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
using System.Collections.Concurrent;

namespace IVO.Implementation.SQL
{
    public sealed class StreamedBlobRepository : IStreamedBlobRepository
    {
        private DataContext db;

        internal DataContext DB { get { return db; } }

        public StreamedBlobRepository(DataContext db)
        {
            this.db = db;
        }

        public async Task<IStreamedBlob[]> PersistBlobs(params PersistingBlob[] blobs)
        {
            // Start persisting blobs:
            Task<IStreamedBlob>[] tasks = new Task<IStreamedBlob>[blobs.Length];
            for (int i = 0; i < blobs.Length; ++i)
            {
                tasks[i] = db.ExecuteNonQueryAsync(new PersistBlob(this, blobs[i]));
            }

            // When all persists are complete, roll up the results from all the tasks into a single array:
            var streamedBlobs = await TaskEx.WhenAll(tasks);

            return streamedBlobs;
        }

        public Task<BlobID[]> DeleteBlobs(params BlobID[] ids)
        {
            Task<BlobID>[] tasks = new Task<BlobID>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = db.ExecuteNonQueryAsync(new DestroyBlob(ids[i]));

            return TaskEx.WhenAll(tasks);
        }

        public Task<IStreamedBlob[]> GetBlobs(params BlobID[] ids)
        {
            IStreamedBlob[] blobs = new IStreamedBlob[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                blobs[i] = new StreamedBlob(this, ids[i]);
            return TaskEx.FromResult(blobs);
        }
    }
}
