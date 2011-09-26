using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using System.IO;
using IVO.Definition.Errors;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace IVO.Implementation.FileSystem
{
    public sealed class TreeRepository : ITreeRepository
    {
        private FileSystem system;

        public TreeRepository(FileSystem system)
        {
            this.system = system;
        }

        #region Private details

        private async Task<Errorable<Tree>> getTree(TreeID id)
        {
            FileInfo fi = system.getPathByID(id);
            if (!fi.Exists) return new TreeIDRecordDoesNotExistError();

            byte[] buf;
            int nr = 0;
            using (var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true))
            {
                // TODO: implement an async buffered Stream:
                buf = new byte[16384];
                nr = await fs.ReadAsync(buf, 0, 16384).ConfigureAwait(continueOnCapturedContext: false);
                if (nr >= 16384)
                {
                    // My, what a large tree you have!
                    throw new NotSupportedException();
                }
            }

            Tree.Builder tb = new Tree.Builder(new List<TreeTreeReference>(), new List<TreeBlobReference>());

            // Parse the Tree:
            using (var ms = new MemoryStream(buf, 0, nr, false))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("tree "))
                    {
                        string linked_treeid = line.Substring(5, (TreeID.ByteArrayLength * 2));
                        string name = line.Substring(6 + (TreeID.ByteArrayLength * 2));
                        tb.Trees.Add(new TreeTreeReference.Builder(name, TreeID.Parse(linked_treeid).Value));
                    }
                    else if (line.StartsWith("blob "))
                    {
                        string linked_blobid = line.Substring(5, (TreeID.ByteArrayLength * 2));
                        string name = line.Substring(6 + (TreeID.ByteArrayLength * 2));
                        tb.Blobs.Add(new TreeBlobReference.Builder(name, BlobID.Parse(linked_blobid).Value));
                    }
                }
            }

            // Create the immutable Tree from the Builder:
            Tree tr = tb;
            // Validate the computed TreeID:
            if (tr.ID != id) return new ComputedTreeIDMismatchError();

            return tr;
        }

        private void persistTree(Tree tr)
        {
            FileInfo fi = system.getPathByID(tr.ID);
            if (fi.Exists) return;

            // Create directory if it doesn't exist:
            if (!fi.Directory.Exists)
            {
                Debug.WriteLine(String.Format("New DIR '{0}'", fi.Directory.FullName));
                fi.Directory.Create();
            }

            // Write the tree contents to the file:
            using (var fs = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                Debug.WriteLine(String.Format("New TREE '{0}'", fi.FullName));
                tr.WriteTo(fs);
            }
        }

        private void deleteTree(TreeID id)
        {
            FileInfo fi = system.getPathByID(id);
            if (!fi.Exists) return;

            fi.Delete();
        }

        private async Task<Errorable<Tree[]>> getTreeRecursively(TreeID id)
        {
            var eroot = await getTree(id).ConfigureAwait(continueOnCapturedContext: false);
            if (eroot.HasErrors) return eroot.Errors;

            var root = eroot.Value;
            var rootArr = new Tree[1] { root };

            if (root.Trees.Length == 0)
                return rootArr;

            Task<Errorable<Tree[]>>[] tasks = new Task<Errorable<Tree[]>>[root.Trees.Length];
            for (int i = 0; i < root.Trees.Length; ++i)
            {
                tasks[i] = getTreeRecursively(root.Trees[i].TreeID);
            }

            // Await all the tree retrievals:
            var allTrees = await TaskEx.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);

            // Roll up all the errors:
            ErrorContainer errors =
                (
                    from etrs in allTrees
                    where etrs.HasErrors
                    select etrs.Errors
                ).Aggregate(new ErrorContainer(), (acc, err) => acc + err);

            if (errors.HasAny) return errors;

            // Flatten out the tree arrays:
            var flattened =
                from etrs in allTrees
                from tr in etrs.Value
                select tr;

            // Return the final array:
            return rootArr.Concat(flattened).ToArray(allTrees.Sum(ta => ta.Value.Length) + 1);
        }

        private async Task<Errorable<TreeIDPathMapping>> getTreeIDByPath(TreeTreePath path)
        {
            // Get the root Tree:
            var eroot = await getTree(path.RootTreeID).ConfigureAwait(continueOnCapturedContext: false);
            if (eroot.HasErrors) return eroot.Errors;
            
            Tree root = eroot.Value;
            ReadOnlyCollection<string> parts = path.Path.Parts;
            if (parts.Count == 0) return new TreeIDPathMapping(path, path.RootTreeID);

            int j = 0;

            // Start descending into child nodes:
            Tree node = root, nextNode;
            while (node != null)
            {
                nextNode = null;

                // Check all child nodes against the next name in the path:
                for (int i = 0; i < node.Trees.Length; ++i)
                    // TODO: specific string comparison logic!
                    if (parts[j] == node.Trees[i].Name)
                    {
                        TreeID nextID = node.Trees[i].TreeID;

                        // Run out of path components? This is it!
                        if (++j == parts.Count) return new TreeIDPathMapping(path, (TreeID?)nextID);

                        // Load up the next node so we can scan through its child nodes:
                        var enextNode = await getTree(nextID).ConfigureAwait(continueOnCapturedContext: false);
                        if (enextNode.HasErrors) return enextNode.Errors;

                        nextNode = enextNode.Value;
                        break;
                    }

                // Attempt to continue:
                node = nextNode;
            }

            // If we got here it means that we didn't find what we were looking for:
            return new TreeIDPathMapping(path, (TreeID?)null);
        }

        #endregion

        public async Task<Errorable<Tree>> PersistTree(TreeID rootid, ImmutableContainer<TreeID, Tree> trees)
        {
            if (trees == null) throw new ArgumentNullException("trees");

            // NOTE: We don't have to persist tree nodes in any particular order here if we implement a filesystem lock.
            Task[] tasks = new Task[trees.Count];
            using (var en = trees.Values.GetEnumerator())
            {
                for (int i = 0; en.MoveNext(); ++i)
                {
                    var tr = en.Current;
                    // FIXME: need concurrency here?
                    tasks[i] = TaskEx.Run(() => persistTree(tr));
                }
            }

            // Wait for all the tasks to complete:
            await TaskEx.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);

            // Return the root tree node:
            return trees[rootid];
        }

        public Task<Errorable<Tree>> GetTree(TreeID id)
        {
            return getTree(id);
        }

        public Task<Errorable<Tree>[]> GetTrees(params TreeID[] ids)
        {
            Task<Errorable<Tree>>[] tasks = new Task<Errorable<Tree>>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                TreeID id = ids[i];
                tasks[i] = getTree(id);
            }
            return TaskEx.WhenAll(tasks);
        }

        public Task<Errorable<TreeIDPathMapping>> GetTreeIDByPath(TreeTreePath path)
        {
            return getTreeIDByPath(path);
        }

        public Task<Errorable<TreeIDPathMapping>[]> GetTreeIDsByPaths(params TreeTreePath[] paths)
        {
            return TaskEx.WhenAll(paths.SelectAsArray(path => getTreeIDByPath(path)));
        }
        
        public async Task<Errorable<TreeID>> DeleteTreeRecursively(TreeID rootid)
        {
            var etrees = await getTreeRecursively(rootid).ConfigureAwait(continueOnCapturedContext: false);
            if (etrees.HasErrors) return etrees.Errors;

            var trees = etrees.Value;

            // TODO: test that 'tr' is captured properly in the lambda.
            Task[] tasks = trees.SelectAsArray(tr => TaskEx.Run(() => deleteTree(tr.ID)));

            await TaskEx.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);
            
            return rootid;
        }

        public async Task<Errorable<TreeTree>> GetTreeRecursively(TreeID rootid)
        {
            // Get all trees recursively:
            var eall = await getTreeRecursively(rootid).ConfigureAwait(continueOnCapturedContext: false);
            if (eall.HasErrors) return eall.Errors;

            Tree[] all = eall.Value;

            // Return them (all[0] is the root):
            return new TreeTree(all[0].ID, new ImmutableContainer<TreeID, Tree>(tr => tr.ID, all));
        }

        public async Task<Errorable<TreeTree>> GetTreeRecursivelyFromPath(TreeTreePath path)
        {
            // Find the TreeID given the path and its root TreeID:
            var etpm = await getTreeIDByPath(path).ConfigureAwait(continueOnCapturedContext: false);
            if (etpm.HasErrors) return etpm.Errors;

            TreeIDPathMapping tpm = etpm.Value;
            if (!tpm.TreeID.HasValue) return null;

            // Now use GetTreeRecursively to do the rest:
            return await GetTreeRecursively(tpm.TreeID.Value).ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
