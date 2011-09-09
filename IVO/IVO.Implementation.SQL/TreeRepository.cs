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
using System.Collections.ObjectModel;

namespace IVO.Implementation.SQL
{
    public class TreeRepository : ITreeRepository
    {
        private DataContext db;

        public TreeRepository(DataContext db)
        {
            this.db = db;
        }

        public async Task<Tree> PersistTree(TreeID rootid, ImmutableContainer<TreeID, Tree> trees)
        {
            // Start a query to check what Trees exist already:
            var existTrees = await db.ExecuteListQueryAsync(new QueryTreesExist(trees.Keys), expectedCapacity: trees.Count);

            // Trees must be created in dependency order!
            HashSet<TreeID> treeIDsToPersistSet = new HashSet<TreeID>(trees.Keys.Except(existTrees));
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

                runner = db.ExecuteNonQueryAsync(new PersistTree(trees[id]));
                await runner;
            }

            return trees[rootid];
        }

        public Task<Tree[]> GetTrees(params TreeID[] ids)
        {
            Task<Tree>[] tasks = new Task<Tree>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                TreeID id = ids[i];
                tasks[i] = db.ExecuteSingleQueryAsync(new QueryTree(id));
            }
            return TaskEx.WhenAll(tasks);
        }

        public Task<Tuple<TreeID, ImmutableContainer<TreeID, Tree>>> GetTreeRecursively(TreeID rootid)
        {
            return db.ExecuteListQueryAsync(new QueryTreeRecursively(rootid));
        }

        public Task<TreeID> DeleteTreeRecursively(TreeID rootid)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<TreeID, ImmutableContainer<TreeID, Tree>>> GetTreeRecursivelyFromPath(TreeTreePath path)
        {
            return db.ExecuteSingleQueryAsync(new QueryTreeRecursivelyByPath(path));
        }

        public Task<Tree> GetTree(TreeID id)
        {
            return db.ExecuteSingleQueryAsync(new QueryTree(id));
        }

        public async Task<TreeIDPathMapping> GetTreeIDByPath(TreeTreePath path)
        {
            var treeIDs = await db.ExecuteSingleQueryAsync(new QueryTreeIDsByPaths(path.RootTreeID, path.Path));
            if (treeIDs.Count == 0) return new TreeIDPathMapping(path, (TreeID?)null);
            return treeIDs[0];
        }

        public async Task<TreeIDPathMapping[]> GetTreeIDsByPaths(params TreeTreePath[] paths)
        {
            // Since we cannot query the database with multiple root TreeIDs at once, group all the
            // paths that share the same root TreeID together and submit those in one query.

            var rootTreeIDGroups = from path in paths group path by path.RootTreeID;

            var tasks = new List<Task<ReadOnlyCollection<TreeIDPathMapping>>>(capacity: paths.Length);
            using (var en = rootTreeIDGroups.GetEnumerator())
            {
                for (int i = 0; en.MoveNext(); ++i)
                {
                    TreeID rootid = en.Current.Key;
                    CanonicalTreePath[] treePaths = en.Current.Select(tr => tr.Path).ToArray();

                    tasks.Add( db.ExecuteSingleQueryAsync(new QueryTreeIDsByPaths(rootid, treePaths)) );
                }
            }

            // Wait for all the queries to come back:
            var allTreeIDs = await TaskEx.WhenAll(tasks);

            // Flatten out the array-of-arrays:
            var flattenedTreeIDs =
                from trArr in allTreeIDs
                from tr in trArr
                select tr;

            return flattenedTreeIDs.ToArray(paths.Length);
        }
    }
}
