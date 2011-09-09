using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using System.IO;
using IVO.Definition.Exceptions;

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

        private async Task<Tree> getTree(TreeID id)
        {
            FileInfo fi = system.getPathByID(id);
            if (!fi.Exists) return null;

            byte[] buf;
            using (var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true))
            {
                // TODO: implement an async buffered Stream:
                buf = new byte[16384];
                int nr = await fs.ReadAsync(buf, 0, 16384);
                if (nr >= 16384)
                {
                    // My, what a large tree you have!
                    throw new NotSupportedException();
                }
            }

            Tree.Builder tb = new Tree.Builder(new List<TreeTreeReference>(), new List<TreeBlobReference>());

            // Parse the Tree:
            using (var ms = new MemoryStream(buf))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("tree "))
                    {
                        int endpos = line.Length - (TreeID.ByteArrayLength * 2) - 1;
                        tb.Trees.Add(new TreeTreeReference.Builder(line.Substring(5, endpos), new TreeID(line.Substring(endpos + 1))));
                    }
                    else if (line.StartsWith("blob "))
                    {
                        int endpos = line.Length - (BlobID.ByteArrayLength * 2) - 1;
                        tb.Blobs.Add(new TreeBlobReference.Builder(line.Substring(5, endpos), new BlobID(line.Substring(endpos + 1))));
                    }
                }
            }

            // Create the immutable Tree from the Builder:
            Tree tr = tb;
            // Validate the computed TreeID:
            if (tr.ID != id) throw new TreeIDMismatchException(tr.ID, id);

            return tr;
        }

        private void persistTree(Tree tr)
        {
            FileInfo fi = system.getPathByID(tr.ID);
            if (fi.Exists) return;

            using (var fs = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                tr.WriteTo(fs);
            }
        }

        #endregion

        public async Task<Tree> PersistTree(TreeID rootid, ImmutableContainer<TreeID, Tree> trees)
        {
            // NOTE: We don't have to persist tree nodes in any particular order here if we implement a filesystem lock.
            Task[] tasks = new Task[trees.Count];
            using (var en = trees.Values.GetEnumerator())
            {
                for (int i = 0; en.MoveNext(); ++i)
                {
                    var tr = en.Current;
                    tasks[i] = TaskEx.Run(() => persistTree(tr));
                }
            }

            // Wait for all the tasks to complete:
            await TaskEx.WhenAll(tasks);

            // Return the root tree node:
            return trees[rootid];
        }

        public Task<Tree[]> GetTrees(params TreeID[] ids)
        {
            Task<Tree>[] tasks = new Task<Tree>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                TreeID id = ids[i];
                tasks[i] = getTree(id);
            }
            return TaskEx.WhenAll(tasks);
        }

        public Task<TreeID> DeleteTreeRecursively(TreeID rootid)
        {
            throw new NotImplementedException();
        }

        public async Task<Tuple<TreeID, ImmutableContainer<TreeID, Tree>>> GetTreeRecursively(TreeID rootid)
        {
            Dictionary<TreeID, Tree> trees = new Dictionary<TreeID, Tree>();

            var root = await getTree(rootid);

            return new Tuple<TreeID, ImmutableContainer<TreeID, Tree>>(rootid, new ImmutableContainer<TreeID, Tree>(trees));
        }

        public Task<Tuple<TreeID, ImmutableContainer<TreeID, Tree>>> GetTreeRecursivelyFromPath(TreeID rootid, CanonicalTreePath path)
        {
            throw new NotImplementedException();
        }
    }
}
