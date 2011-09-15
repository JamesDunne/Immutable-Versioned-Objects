using System.Threading.Tasks;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using IVO.Implementation.SQL.Queries;

namespace IVO.Implementation.SQL
{
    public sealed class TreePathStreamedBlobRepository : ITreePathStreamedBlobRepository
    {
        private DataContext db;
        private StreamedBlobRepository blrepo;

        public TreePathStreamedBlobRepository(DataContext db, StreamedBlobRepository blrepo)
        {
            this.db = db;
            this.blrepo = blrepo;
        }

        public Task<TreePathStreamedBlob[]> GetBlobsByTreePaths(params TreeBlobPath[] treePaths)
        {
            Task<TreePathStreamedBlob>[] tasks = new Task<TreePathStreamedBlob>[treePaths.Length];
            for (int i = 0; i < treePaths.Length; ++i)
                tasks[i] = db.ExecuteSingleQueryAsync(new QueryBlobByPath(blrepo, treePaths[i]));
            return TaskEx.WhenAll(tasks);
        }

        public Task<TreePathStreamedBlob> GetBlobByTreePath(TreeBlobPath treePath)
        {
            return db.ExecuteSingleQueryAsync(new QueryBlobByPath(blrepo, treePath));
        }
    }
}
