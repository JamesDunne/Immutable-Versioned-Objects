using System.Threading.Tasks;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using IVO.Implementation.SQL.Queries;
using IVO.Definition.Errors;

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

        public async Task<Errorable<TreePathStreamedBlob>[]> GetBlobsByTreePaths(params TreeBlobPath[] treePaths)
        {
            var tasks = new Task<Errorable<TreePathStreamedBlob>>[treePaths.Length];
            for (int i = 0; i < treePaths.Length; ++i)
                tasks[i] = db.ExecuteSingleQueryAsync(new QueryBlobByPath(blrepo, treePaths[i]));
            return await Task.WhenAll(tasks);
        }

        public Task<Errorable<TreePathStreamedBlob>> GetBlobByTreePath(TreeBlobPath treePath)
        {
            return db.ExecuteSingleQueryAsync(new QueryBlobByPath(blrepo, treePath));
        }
    }
}
