using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asynq;
using IVO.Implementation.SQL;
using IVO.Implementation.SQL.Persists;
using IVO.Implementation.SQL.Queries;
using IVO.Definition;
using IVO.Definition.Models;
using IVO.Definition.Containers;
using IVO.Definition.Repositories;

namespace IVO.Implementation.SQL
{
    public sealed class BlobRepository : IBlobRepository
    {
        private DataContext db;

        public BlobRepository(DataContext db)
        {
            this.db = db;
        }

        public Task<Blob>[] PersistBlobs(params Blob[] blobs)
        {
            Task<Blob>[] tasks = new Task<Blob>[blobs.Length];
            for (int i = 0; i < blobs.Length; ++i)
                tasks[i] = db.ExecuteNonQueryAsync(new PersistBlob(blobs[i]));
            return tasks;
        }

        public Task<BlobID>[] DeleteBlobs(params BlobID[] ids)
        {
            Task<BlobID>[] tasks = new Task<BlobID>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = db.ExecuteNonQueryAsync(new DestroyBlob(ids[i]));
            return tasks;
        }

        public Task<Blob>[] GetBlobs(params BlobID[] ids)
        {
            Task<Blob>[] tasks = new Task<Blob>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = db.ExecuteSingleQueryAsync(new QueryBlob(ids[i]));
            return tasks;
        }
    }
}
