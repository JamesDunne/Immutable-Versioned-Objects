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

        public Task<Blob[]> PersistBlobs(params Blob[] blobs)
        {
            BlobID[] allIDs = blobs.Select(b => b.ID).ToArray(blobs.Length);
            var blobLookup = blobs.ToDictionary(b => b.ID);

            var wait = db.ExecuteListQueryAsync(new QueryBlobsExist(blobs.Select(b => b.ID)))
                .ContinueWith((existBlobs) =>
                {
                    // Find the BlobIDs to persist:
                    var blobsToPersist = allIDs.Except(existBlobs.Result).ToList(allIDs.Length - existBlobs.Result.Count);

                    // Early-out case:
                    if (blobsToPersist.Count == 0)
                        return Task.Factory.StartNew(() => new Blob[0]);
                    
                    // Start persisting blobs:
                    Task<Blob>[] tasks = new Task<Blob>[blobsToPersist.Count];
                    for (int i = 0; i < blobsToPersist.Count; ++i)
                        tasks[i] = db.ExecuteNonQueryAsync(new PersistBlob(blobLookup[blobsToPersist[i]]));

                    // When all persists are complete, roll up the results from all the tasks into a single array:
                    return Task.Factory.ContinueWhenAll(tasks, ts => ts.Select(t => t.Result).ToArray(blobsToPersist.Count));
                }).Unwrap();
            return wait;
        }

        public Task<BlobID[]> DeleteBlobs(params BlobID[] ids)
        {
            Task<BlobID>[] tasks = new Task<BlobID>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = db.ExecuteNonQueryAsync(new DestroyBlob(ids[i]));
            return Task.Factory.ContinueWhenAll(tasks, ts => ts.Select(t => t.Result).ToArray(ids.Length));
        }

        public Task<Blob[]> GetBlobs(params BlobID[] ids)
        {
            Task<Blob>[] tasks = new Task<Blob>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = db.ExecuteSingleQueryAsync(new QueryBlob(ids[i]));
            return Task.Factory.ContinueWhenAll(tasks, ts => ts.Select(t => t.Result).ToArray(ids.Length));
        }
    }
}
