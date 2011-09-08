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
    public sealed class BlobRepository : IBlobRepository
    {
        private DataContext db;

        internal DataContext DB { get { return db; } }

        public BlobRepository(DataContext db)
        {
            this.db = db;
        }

        public async Task<IStreamedBlob[]> PersistBlobs(params PersistingBlob[] blobs)
        {
            var blobIndexLookup = new ConcurrentDictionary<BlobID, int>(Environment.ProcessorCount * 2, blobs.Length);
            IStreamedBlob[] streamedBlobs = new IStreamedBlob[blobs.Length];

            // Compute all our BlobIDs first:
            Task<BlobID>[] computeIDTasks = new Task<BlobID>[blobs.Length];
            for (int i = 0; i < blobs.Length; ++i)
            {
                int index = i;
                PersistingBlob blob = blobs[index];
                if (blob.ID.HasValue)
                    computeIDTasks[index] = TaskEx.FromResult(blob.ID.Value);
                else
                    computeIDTasks[index] = TaskEx.Run((Func<BlobID>)blob.ComputeID);

                // Update the lookup dictionary:
                computeIDTasks[index] = computeIDTasks[index].ContinueWith(blidTask => { blobIndexLookup.AddOrUpdate(blidTask.Result, index, (k, v) => index); return blidTask.Result; });
                streamedBlobs[index] = new StreamedBlob(this, blob.ComputedID);
            }
            await TaskEx.WhenAll(computeIDTasks);

            // Query which BlobIDs already exist:
            var existBlobs = await db.ExecuteListQueryAsync(new QueryBlobsExist(blobs.Select(bl => bl.ID.Value)), expectedCapacity: blobs.Length);

            // Find the BlobIDs to persist:
            var blobsToPersist = blobs.Select(bl => bl.ID.Value).Except(existBlobs).ToList(blobs.Length - existBlobs.Count);

            // Early-out case:
            if (blobsToPersist.Count == 0)
                return streamedBlobs;

            // Start persisting blobs:
            Task<PersistingBlob>[] tasks = new Task<PersistingBlob>[blobsToPersist.Count];
            for (int i = 0; i < blobsToPersist.Count; ++i)
                tasks[i] = db.ExecuteNonQueryAsync(new PersistBlob(blobs[blobIndexLookup[blobsToPersist[i]]]));

            // When all persists are complete, roll up the results from all the tasks into a single array:
            await TaskEx.WhenAll(tasks);
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

        public Task<TreePathStreamedBlob[]> GetBlobsByTreePaths(params TreePath[] treePaths)
        {
            Task<TreePathStreamedBlob>[] tasks = new Task<TreePathStreamedBlob>[treePaths.Length];
            for (int i = 0; i < treePaths.Length; ++i)
                tasks[i] = db.ExecuteSingleQueryAsync(new QueryBlobByPath(this, treePaths[i]));
            return TaskEx.WhenAll(tasks);
        }
    }
}
