using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using System.IO;

namespace IVO.Implementation.FileSystem
{
    public sealed class TreeRepository : ITreeRepository
    {
        private FileSystem system;
        
        public TreeRepository(FileSystem system)
        {
            this.system = system;
        }

        private async Task<Tree> persistTree(Tree tr)
        {
            FileInfo fi = system.getPathByID(tr.ID);
            if (fi.Exists) return tr;

            using (var fs = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                tr.WriteTo(fs);
            }

            return tr;
        }

        public async Task<Tree> PersistTree(TreeID rootid, ImmutableContainer<TreeID, Tree> trees)
        {
            // NOTE: We don't have to persist tree nodes in any particular order here if we implement a filesystem lock.
            Task<Tree>[] tasks = new Task<Tree>[trees.Count];
            using (var en = trees.Values.GetEnumerator())
            {
                for (int i = 0; en.MoveNext(); ++i)
                {
                    var tr = en.Current;
                    tasks[i] = TaskEx.RunEx(() => persistTree(tr));
                }
            }

            // Wait for all the tasks to complete:
            await TaskEx.WhenAll(tasks);

            // Return the root tree node:
            return trees[rootid];
        }

        public Task<TreeID> DeleteTreeRecursively(TreeID rootid)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<TreeID, ImmutableContainer<TreeID, Tree>>> GetTreeRecursively(TreeID rootid)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<TreeID, ImmutableContainer<TreeID, Tree>>> GetTreeRecursivelyFromPath(TreeID rootid, CanonicalTreePath path)
        {
            throw new NotImplementedException();
        }
    }
}
