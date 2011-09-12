using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Repositories;

namespace IVO.Implementation.FileSystem
{
    public sealed class TreePathStreamedBlobRepository : ITreePathStreamedBlobRepository
    {
        private FileSystem system;
        private StreamedBlobRepository blrepo;
        private TreeRepository trrepo;

        public TreePathStreamedBlobRepository(FileSystem system, TreeRepository trrepo = null, StreamedBlobRepository blrepo = null)
        {
            this.system = system;
            this.trrepo = trrepo ?? new TreeRepository(system);
            this.blrepo = blrepo ?? new StreamedBlobRepository(system);
        }

        public Task<TreePathStreamedBlob[]> GetBlobsByTreePaths(params TreeBlobPath[] treePaths)
        {
            throw new NotImplementedException();
        }
    }
}
