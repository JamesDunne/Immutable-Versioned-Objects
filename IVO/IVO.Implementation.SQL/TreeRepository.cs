using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asynq;
using IVO.Data;
using IVO.Data.Persists;
using IVO.Data.Queries;
using IVO.Definition;
using IVO.Definition.Models;
using IVO.Definition.Containers;
using IVO.Definition.Repositories;
using System.Diagnostics;

namespace IVO.Implementation.SQL
{
    public class TreeRepository : ITreeRepository
    {
        private DataContext db;

        public TreeRepository(DataContext db)
        {
            this.db = db;
        }

        public Task<Tree> PersistTree(TreeID rootid, TreeContainer trees, BlobContainer blobs)
        {
            // Start queries to check what exists already:
            var existBlobs = db.ExecuteListQueryAsync(new QueryBlobsExist(blobs.Keys), expectedCapacity: blobs.Count);
            var existTrees = db.ExecuteListQueryAsync(new QueryTreesExist(trees.Keys), expectedCapacity: trees.Count);

            Debug.WriteLine("{0,3}: Awaiting existBlobs...", Task.CurrentId);
            return existBlobs.ContinueWith((blTask) =>
            {
                Debug.WriteLine("{0,3}: existBlobs complete...", Task.CurrentId);

                // First, persist blobs that don't exist:
                //Console.WriteLine("Waiting for blob exists...");
                BlobID[] blobIDsToPersist = blobs.Keys.Except(blTask.Result).ToArray();

                // Blobs may be persisted in any order; there are no dependencies between blobs:
                Task[] waiters = new Task[blobIDsToPersist.Length + 1];
                for (int i = 0; i < blobIDsToPersist.Length; ++i)
                {
                    BlobID id = blobIDsToPersist[i];

                    Debug.WriteLine("{0,3}: PERSIST blob {1}", Task.CurrentId, id.ToString());
                    waiters[i] = db.ExecuteNonQueryAsync(new PersistBlob(blobs[id]));
                }

                // Also wait for the existTrees query to come back:
                waiters[waiters.Length - 1] = existTrees;

                Debug.WriteLine("{0,3}: Awaiting existsTrees and any blob persists...", Task.CurrentId);
                // When everything above completes, do this:
                return Task.Factory.ContinueWhenAll(waiters, (tasks) =>
                {
                    // Next, persist trees that don't exist:
                    Debug.WriteLine("{0,3}: existsTrees and blob persists complete.", Task.CurrentId);

                    // Trees must be created in dependency order!
                    HashSet<TreeID> treeIDsToPersistSet = new HashSet<TreeID>(trees.Keys.Except(existTrees.Result));
                    Stack<TreeID> treeIDsToPersist = new Stack<TreeID>(treeIDsToPersistSet.Count);

                    // Run through trees from root to leaf:
                    Queue<TreeID> treeQueue = new Queue<TreeID>();
                    treeQueue.Enqueue(rootid);

                    Debug.WriteLine("{0,3}: Ordering trees in dependent order for persistence...", Task.CurrentId);
                    while (treeQueue.Count > 0)
                    {
                        TreeID trID = treeQueue.Dequeue();
                        if (treeIDsToPersistSet.Contains(trID))
                            treeIDsToPersist.Push(trID);

                        Tree tr = trees[trID];
                        if (tr.Trees == null) continue;

                        foreach (TreeTreeReference r in tr.Trees)
                        {
                            treeQueue.Enqueue(r.TreeID);
                        }
                    }

                    // Asynchronously persist the trees in dependency order:
                    // FIXME: asynchronous fan-out per depth level
                    Task<Tree> waiter = null, runner = null;
                    while (treeIDsToPersist.Count > 0)
                    {
                        TreeID id = treeIDsToPersist.Pop();

                        Debug.WriteLine("{0,3}: PERSIST tree {1}", Task.CurrentId, id.ToString());

                        if (runner == null) waiter = runner = db.ExecuteNonQueryAsync(new PersistTree(trees[id]));
                        else runner = runner.ContinueWith(r => db.ExecuteNonQueryAsync(new PersistTree(trees[id]))).Unwrap();
                    }

                    if (runner != null)
                        // Return the last chained task and our final result is our root Tree:
                        return runner.ContinueWith(x => trees[rootid], TaskContinuationOptions.ExecuteSynchronously);
                    else
                    {
                        Task<Tree> dummy = new Task<Tree>(() => trees[rootid]);
                        dummy.Start();
                        return dummy;
                    }
                }).Unwrap();
            }).Unwrap();
        }

        public Task<Tuple<TreeID, TreeContainer>> RetrieveTreeRecursively(TreeID rootid)
        {
            return db.ExecuteListQueryAsync(new QueryTreeRecursively(rootid));
        }

        public Task<TreeID> DeleteTreeRecursively(TreeID rootid)
        {
            throw new NotImplementedException();
        }
    }
}
