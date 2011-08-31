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

        public Task<Tree> PersistTree(TreeID rootid, TreeContainer trees)
        {
            return
                // Start a query to check what Trees exist already:
                db.ExecuteListQueryAsync(new QueryTreesExist(trees.Keys), expectedCapacity: trees.Count)
                .ContinueWith(existTrees =>
                {
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
                    Task<Tree> runner = null;
                    while (treeIDsToPersist.Count > 0)
                    {
                        TreeID id = treeIDsToPersist.Pop();

                        Debug.WriteLine("{0,3}: PERSIST tree {1}", Task.CurrentId, id.ToString());

                        if (runner == null) runner = db.ExecuteNonQueryAsync(new PersistTree(trees[id]));
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
