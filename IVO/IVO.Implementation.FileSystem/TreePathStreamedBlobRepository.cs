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

        public TreePathStreamedBlobRepository(FileSystem system, StreamedBlobRepository blrepo, TreeRepository trrepo)
        {
            this.system = system;
            this.blrepo = blrepo;
        }

        public Task<TreePathStreamedBlob[]> GetBlobsByTreePaths(params TreeBlobPath[] treePaths)
        {
            throw new NotImplementedException();
        }
    }
}
