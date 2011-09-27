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
using IVO.Definition.Errors;

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

        public async Task<Errorable<IStreamedBlob>[]> PersistBlobs(params PersistingBlob[] blobs)
        {
            // Start persisting blobs:
            var tasks = new Task<Errorable<IStreamedBlob>>[blobs.Length];
            for (int i = 0; i < blobs.Length; ++i)
            {
                tasks[i] = db.ExecuteNonQueryAsync(new PersistBlob(this, blobs[i]));
            }

            // When all persists are complete, roll up the results from all the tasks into a single array:
            var streamedBlobs = await TaskEx.WhenAll(tasks);

            return streamedBlobs;
        }

        public Task<Errorable<BlobID>[]> DeleteBlobs(params BlobID[] ids)
        {
            var tasks = new Task<Errorable<BlobID>>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = db.ExecuteNonQueryAsync(new DestroyBlob(ids[i]));

            return TaskEx.WhenAll(tasks);
        }

        public Task<Errorable<IStreamedBlob>[]> GetBlobs(params BlobID[] ids)
        {
            var blobs = new Errorable<IStreamedBlob>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                blobs[i] = new StreamedBlob(this, ids[i]);
            return TaskEx.FromResult(blobs);
        }

        public Task<Errorable<IStreamedBlob>> PersistBlob(PersistingBlob blob)
        {
            return db.ExecuteNonQueryAsync(new PersistBlob(this, blob));
        }

        public Task<Errorable<BlobID>> DeleteBlob(BlobID id)
        {
            return db.ExecuteNonQueryAsync(new DestroyBlob(id));
        }

        public Task<Errorable<IStreamedBlob>> GetBlob(BlobID id)
        {
            return TaskEx.FromResult( (Errorable<IStreamedBlob>) new StreamedBlob(this, id) );
        }
    }
}
