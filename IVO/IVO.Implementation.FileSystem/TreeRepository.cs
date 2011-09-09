using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public Task<Tree> PersistTree(TreeID rootid, Definition.Containers.ImmutableContainer<TreeID, Tree> trees)
        {
            throw new NotImplementedException();
        }

        public Task<TreeID> DeleteTreeRecursively(TreeID rootid)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<TreeID, Definition.Containers.ImmutableContainer<TreeID, Tree>>> GetTreeRecursively(TreeID rootid)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<TreeID, Definition.Containers.ImmutableContainer<TreeID, Tree>>> GetTreeRecursivelyFromPath(TreeID rootid, CanonicalTreePath path)
        {
            throw new NotImplementedException();
        }
    }
}
