using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Containers;
using IVO.Definition.Errors;
using IVO.Definition.Models;
using IVO.Definition.Repositories;

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

        private async Task<Errorable<TreeNode>> getTree(TreeID id)
        {
            FileInfo fi = system.getPathByID(id);
            if (!fi.Exists) return new TreeIDRecordDoesNotExistError(id);

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

            TreeNode.Builder tb = new TreeNode.Builder(new List<TreeTreeReference>(), new List<TreeBlobReference>());

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

                        // Attempt to parse the TreeID and verify its existence:
                        Errorable<TreeID> trid = TreeID.TryParse(linked_treeid);
                        if (trid.HasErrors) return trid.Errors;
                        if (!system.getPathByID(trid.Value).Exists) return new TreeIDRecordDoesNotExistError(trid.Value);
                        tb.Trees.Add(new TreeTreeReference.Builder(name, trid.Value));
                    }
                    else if (line.StartsWith("blob "))
                    {
                        string linked_blobid = line.Substring(5, (TreeID.ByteArrayLength * 2));
                        string name = line.Substring(6 + (TreeID.ByteArrayLength * 2));

                        // Attempt to parse the BlobID and verify its existence:
                        Errorable<BlobID> blid = BlobID.TryParse(linked_blobid);
                        if (blid.HasErrors) return blid.Errors;
                        if (!system.getPathByID(blid.Value).Exists) return new BlobIDRecordDoesNotExistError(blid.Value);
                        tb.Blobs.Add(new TreeBlobReference.Builder(name, blid.Value));
                    }
                }
            }

            // Create the immutable Tree from the Builder:
            TreeNode tr = tb;
            // Validate the computed TreeID:
            if (tr.ID != id) return new ComputedTreeIDMismatchError(tr.ID, id);

            return tr;
        }

        private Errorable<TreeNode> persistTree(TreeNode tr)
        {
            Debug.Assert(tr != null);

            // Check that all referenced blobs are already persisted:
            foreach (var trbl in tr.Blobs)
            {
                if (!system.getPathByID(trbl.BlobID).Exists)
                    return new BlobIDRecordDoesNotExistError(trbl.BlobID);
            }

            // Check that all referenced blobs are already persisted:
            foreach (var trtr in tr.Trees)
            {
                if (!system.getPathByID(trtr.TreeID).Exists)
                    return new TreeIDRecordDoesNotExistError(trtr.TreeID);
            }

            FileInfo fi = system.getPathByID(tr.ID);
            // TODO: maybe a TreeIDRecordAlreadyExistsError?
            if (fi.Exists) return tr;

            // Create directory if it doesn't exist:
            if (!fi.Directory.Exists)
            {
                Debug.WriteLine(String.Format("New DIR '{0}'", fi.Directory.FullName));
                fi.Directory.Create();
            }

            // Write the tree contents to the file:
            lock (FileSystem.SystemLock)
            {
                if (!fi.Exists)
                    using (var fs = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    {
                        Debug.WriteLine(String.Format("New TREE '{0}'", fi.FullName));
                        tr.WriteTo(fs);
                    }
            }

            return tr;
        }

        private void deleteTree(TreeID id)
        {
            FileInfo fi = system.getPathByID(id);
            lock (FileSystem.SystemLock)
            {
                if (!fi.Exists) return;

                fi.Delete();
            }
        }

        private async Task<Errorable<TreeNode[]>> getTreeRecursively(TreeID id)
        {
            var eroot = await getTree(id).ConfigureAwait(continueOnCapturedContext: false);
            if (eroot.HasErrors) return eroot.Errors;

            var root = eroot.Value;
            var rootArr = new TreeNode[1] { root };

            if (root.Trees.Length == 0)
                return rootArr;

            Task<Errorable<TreeNode[]>>[] tasks = new Task<Errorable<TreeNode[]>>[root.Trees.Length];
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

            TreeNode root = eroot.Value;
            ReadOnlyCollection<string> parts = path.Path.Parts;
            if (parts.Count == 0) return new TreeIDPathMapping(path, path.RootTreeID);

            int j = 0;

            // Start descending into child nodes:
            TreeNode node = root, nextNode;
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

        public async Task<Errorable<TreeNode>> PersistTree(TreeID rootid, ImmutableContainer<TreeID, TreeNode> trees)
        {
            if (trees == null) throw new ArgumentNullException("trees");
            // TODO: better return value than `null`
            if (trees.Count == 0) return (TreeNode)null;

            // This code scans the tree breadth-first and builds a reversed depth-ordered stack:

            var reverseDepthOrder = new { id = rootid, depth = 0 }.StackOf(trees.Count);
            reverseDepthOrder.Pop();

            var breadthFirstQueue = new { id = rootid, depth = 0 }.QueueOf(trees.Count);
            while (breadthFirstQueue.Count > 0)
            {
                var curr = breadthFirstQueue.Dequeue();
                // Add it to the reverse stack:
                reverseDepthOrder.Push(curr);

                TreeNode node;
                if (!trees.TryGetValue(curr.id, out node))
                {
                    // TODO: didn't find the TreeID in the given collection, assume already persisted?
                    continue;
                }

                // Queue up the child TreeIDs:
                foreach (var trtr in node.Trees)
                    breadthFirstQueue.Enqueue(new { id = trtr.TreeID, depth = curr.depth + 1 });
            }

            // This code takes the reverse depth-ordered stack and persists the tree nodes in groups per depth level.
            // This ensures that all child nodes across the breadth of the tree at each depth level are persisted
            // before moving up to their parents.

            List<Task<Errorable<TreeNode>>> persistTasks = new List<Task<Errorable<TreeNode>>>();
            HashSet<TreeID> isPersisting = new HashSet<TreeID>();

            int lastDepth = reverseDepthOrder.Peek().depth;
            foreach (var curr in reverseDepthOrder)
            {
                Debug.WriteLine(String.Format("{0}: {1}", curr.depth, curr.id.ToString(firstLength: 7)));
                // An invariant of the algorithm, enforced via assert:
                Debug.Assert(curr.depth <= lastDepth);

                // Did we move to the next depth group:
                if (curr.depth != lastDepth)
                {
                    Debug.WriteLine(String.Format("Awaiting depth group {0}...", lastDepth));
                    // Wait for the last depth group to finish persisting:
                    await TaskEx.WhenAll(persistTasks);
                    // TODO: roll up errors!

                    // Start a new depth group:
                    persistTasks = new List<Task<Errorable<TreeNode>>>();
                }

                // Don't re-persist the same TreeID (this is a legit case - the same TreeID may be seen in different nodes of the tree):
                if (isPersisting.Contains(curr.id))
                {
                    Debug.WriteLine(String.Format("Already persisting {0}", curr.id.ToString(firstLength: 7)));

                    // Keep track of the last depth level:
                    lastDepth = curr.depth;
                    continue;
                }

                // Get the TreeNode and persist it:
                TreeNode node = trees[curr.id];
                isPersisting.Add(curr.id);
                
                // Fire up a task to persist this tree node:
                var tsk = TaskEx.Run(() => persistTree(node));
                
                // Add the task to the depth group to await:
                Debug.WriteLine(String.Format("Adding to depth group {0}...", curr.depth));
                persistTasks.Add(tsk);

                // Keep track of the last depth level:
                lastDepth = curr.depth;
            }

            // The final depth group should be depth 0 with at most 1 element: the root node.
            Debug.Assert(lastDepth == 0);
            if (persistTasks.Count > 0)
            {
                // Await the last group (the root node):
                Debug.WriteLine(String.Format("Awaiting depth group {0}...", lastDepth));
                await TaskEx.WhenAll(persistTasks);
            }

            // Return the root TreeNode:
            return persistTasks[0].Result;
        }

        public Task<Errorable<TreeNode>> GetTree(TreeID id)
        {
            return getTree(id);
        }

        public Task<Errorable<TreeNode>[]> GetTrees(params TreeID[] ids)
        {
            Task<Errorable<TreeNode>>[] tasks = new Task<Errorable<TreeNode>>[ids.Length];
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

            TreeNode[] all = eall.Value;

            // Return them (all[0] is the root):
            return new TreeTree(all[0].ID, new ImmutableContainer<TreeID, TreeNode>(tr => tr.ID, all));
        }

        public async Task<Errorable<TreeTree>> GetTreeRecursivelyFromPath(TreeTreePath path)
        {
            // Find the TreeID given the path and its root TreeID:
            var etpm = await getTreeIDByPath(path).ConfigureAwait(continueOnCapturedContext: false);
            if (etpm.HasErrors) return etpm.Errors;

            TreeIDPathMapping tpm = etpm.Value;
            // TODO: TreePathNotFoundError
            if (!tpm.TreeID.HasValue) return (TreeTree)null;

            // Now use GetTreeRecursively to do the rest:
            return await GetTreeRecursively(tpm.TreeID.Value).ConfigureAwait(continueOnCapturedContext: false);
        }

        public Task<Errorable<TreeID>> ResolvePartialID(TreeID.Partial id)
        {
            FileInfo[] fis = system.getPathsByPartialID(id);
            if (fis.Length == 1) return TaskEx.FromResult(TreeID.TryParse(id.ToString().Substring(0, 2) + fis[0].Name));
            if (fis.Length == 0) return TaskEx.FromResult((Errorable<TreeID>)new TreeIDPartialNoResolutionError(id));
            return TaskEx.FromResult((Errorable<TreeID>)new TreeIDPartialAmbiguousResolutionError(id, fis.SelectAsArray(f => TreeID.TryParse(id.ToString().Substring(0, 2) + f.Name).Value)));
        }

        public Task<Errorable<TreeID>[]> ResolvePartialIDs(params TreeID.Partial[] ids)
        {
            return TaskEx.WhenAll(ids.SelectAsArray(id => ResolvePartialID(id)));
        }
    }
}
