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

        public async Task<ImmutableContainer<BlobID, Blob>> PersistBlobs(ImmutableContainer<BlobID, Blob> blobs)
        {
            var existBlobs = await db.ExecuteListQueryAsync(new QueryBlobsExist(blobs.Keys), expectedCapacity: blobs.Count);

            // Find the BlobIDs to persist:
            var blobsToPersist = blobs.Keys.Except(existBlobs).ToList(blobs.Count - existBlobs.Count);

            // Early-out case:
            if (blobsToPersist.Count == 0)
                return blobs;

            // Start persisting blobs:
            Task<Blob>[] tasks = new Task<Blob>[blobsToPersist.Count];
            for (int i = 0; i < blobsToPersist.Count; ++i)
                tasks[i] = db.ExecuteNonQueryAsync(new PersistBlob(blobs[blobsToPersist[i]]));

            // When all persists are complete, roll up the results from all the tasks into a single array:
            await TaskEx.WhenAll(tasks);
            return blobs;
        }

        public async Task<BlobID[]> DeleteBlobs(params BlobID[] ids)
        {
            Task<BlobID>[] tasks = new Task<BlobID>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = db.ExecuteNonQueryAsync(new DestroyBlob(ids[i]));

            var blobIDs = await TaskEx.WhenAll(tasks);
            return blobIDs;
        }

        public async Task<Blob[]> GetBlobs(params BlobID[] ids)
        {
            Task<Blob>[] tasks = new Task<Blob>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = db.ExecuteSingleQueryAsync(new QueryBlob(ids[i]));

            var blobs = await TaskEx.WhenAll(tasks);
            return blobs;
        }

        public Task<BlobTreePath> GetBlobByAbsolutePath(TreeID rootid, CanonicalBlobPath path)
        {
            return db.ExecuteSingleQueryAsync(new QueryBlobByPath(rootid, path));
        }
    }
}
