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
using IVO.Definition.Errors;

namespace IVO.Implementation.SQL
{
    public class TreeRepository : ITreeRepository
    {
        private DataContext db;

        public TreeRepository(DataContext db)
        {
            this.db = db;
        }

        public async Task<Errorable<TreeNode>> PersistTree(TreeID rootid, ImmutableContainer<TreeID, TreeNode> trees)
        {
            if (trees == null) throw new ArgumentNullException("trees");
            // TODO: better return value than `null`
            if (trees.Count == 0) return (TreeNode)null;

            // Start a query to check what Trees exist already:
            var existTrees = await db.ExecuteListQueryAsync(new QueryTreesExist(trees.Keys), expectedCapacity: trees.Count);

            // This code scans the tree breadth-first and builds a reversed depth-ordered stack:

            var reverseDepthOrder = new { id = rootid, depth = 0 }.StackOf(trees.Count);
            reverseDepthOrder.Pop();

            var breadthFirstQueue = new { id = rootid, depth = 0 }.QueueOf(trees.Count);
            while (breadthFirstQueue.Count > 0)
            {
                var curr = breadthFirstQueue.Dequeue();
                // Add it to the reverse stack:
                reverseDepthOrder.Push(curr);

                TreeNode node;
                if (!trees.TryGetValue(curr.id, out node))
                {
                    // TODO: didn't find the TreeID in the given collection, assume already persisted?
                    continue;
                }

                // Queue up the child TreeIDs:
                foreach (var trtr in node.Trees)
                    breadthFirstQueue.Enqueue(new { id = trtr.TreeID, depth = curr.depth + 1 });
            }

            // This code takes the reverse depth-ordered stack and persists the tree nodes in groups per depth level.
            // This ensures that all child nodes across the breadth of the tree at each depth level are persisted
            // before moving up to their parents.

            List<Task<Errorable<TreeNode>>> persistTasks = new List<Task<Errorable<TreeNode>>>();
            // Initialize the `isPersisting` set with the set of TreeIDs that already exist.
            HashSet<TreeID> isPersisting = new HashSet<TreeID>(existTrees);

            int lastDepth = reverseDepthOrder.Peek().depth;
            foreach (var curr in reverseDepthOrder)
            {
                Debug.WriteLine(String.Format("{0}: {1}", curr.depth, curr.id.ToString(firstLength: 7)));
                // An invariant of the algorithm, enforced via assert:
                Debug.Assert(curr.depth <= lastDepth);

                // Did we move to the next depth group:
                if ((persistTasks.Count > 0) && (curr.depth != lastDepth))
                {
                    Debug.WriteLine(String.Format("Awaiting depth group {0}...", lastDepth));
                    // Wait for the last depth group to finish persisting:
                    await TaskEx.WhenAll(persistTasks);

                    // Start a new depth group:
                    persistTasks = new List<Task<Errorable<TreeNode>>>();
                }

                // Don't re-persist the same TreeID (this is a legit case - the same TreeID may be seen in different nodes of the tree):
                if (isPersisting.Contains(curr.id))
                {
                    Debug.WriteLine(String.Format("Already persisting {0}", curr.id.ToString(firstLength: 7)));

                    // Keep track of the last depth level:
                    lastDepth = curr.depth;
                    continue;
                }

                // Get the TreeNode and persist it:
                TreeNode node = trees[curr.id];
                isPersisting.Add(curr.id);

                // Fire up a task to persist this tree node:
                var tsk = db.ExecuteNonQueryAsync(new PersistTree(node));

                // Add the task to the depth group to await:
                Debug.WriteLine(String.Format("Adding to depth group {0}...", curr.depth));
                persistTasks.Add(tsk);

                // Keep track of the last depth level:
                lastDepth = curr.depth;
            }

            Debug.Assert(lastDepth == 0);
            if (persistTasks.Count > 0)
            {
                // Await the last group (the root node):
                Debug.WriteLine(String.Format("Awaiting depth group {0}...", lastDepth));
                await TaskEx.WhenAll(persistTasks);
            }

            // Return the root TreeNode:
            return trees[rootid];
        }

        public Task<Errorable<TreeNode>[]> GetTrees(params TreeID[] ids)
        {
            Task<Errorable<TreeNode>>[] tasks = new Task<Errorable<TreeNode>>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                TreeID id = ids[i];
                tasks[i] = db.ExecuteSingleQueryAsync(new QueryTree(id));
            }
            return TaskEx.WhenAll(tasks);
        }

        public Task<Errorable<TreeTree>> GetTreeRecursively(TreeID rootid)
        {
            return db.ExecuteListQueryAsync(new QueryTreeRecursively(rootid));
        }

        public Task<Errorable<TreeID>> DeleteTreeRecursively(TreeID rootid)
        {
            throw new NotImplementedException();
        }

        public Task<Errorable<TreeTree>> GetTreeRecursivelyFromPath(TreeTreePath path)
        {
            return db.ExecuteSingleQueryAsync(new QueryTreeRecursivelyByPath(path));
        }

        public Task<Errorable<TreeNode>> GetTree(TreeID id)
        {
            return db.ExecuteSingleQueryAsync(new QueryTree(id));
        }

        public async Task<Errorable<TreeIDPathMapping>> GetTreeIDByPath(TreeTreePath path)
        {
            var treeIDs = await db.ExecuteSingleQueryAsync(new QueryTreeIDsByPaths(path.RootTreeID, path.Path));
            if (treeIDs.Count == 0) return new TreeIDPathMapping(path, (TreeID?)null);
            return treeIDs[0];
        }

        public async Task<Errorable<TreeIDPathMapping>[]> GetTreeIDsByPaths(params TreeTreePath[] paths)
        {
            // Since we cannot query the database with multiple root TreeIDs at once, group all the
            // paths that share the same root TreeID together and submit those in one query.

            var rootTreeIDGroups = from path in paths group path by path.RootTreeID;

            var tasks = new List<Task<ReadOnlyCollection<Errorable<TreeIDPathMapping>>>>(capacity: paths.Length);
            using (var en = rootTreeIDGroups.GetEnumerator())
            {
                for (int i = 0; en.MoveNext(); ++i)
                {
                    TreeID rootid = en.Current.Key;
                    CanonicalTreePath[] treePaths = en.Current.Select(tr => tr.Path).ToArray();

                    tasks.Add(db.ExecuteSingleQueryAsync(new QueryTreeIDsByPaths(rootid, treePaths)));
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

        public async Task<Errorable<TreeID>> ResolvePartialID(TreeID.Partial id)
        {
            var resolvedIDs = await db.ExecuteListQueryAsync(new ResolvePartialTreeID(id));
            if (resolvedIDs.Length == 1) return resolvedIDs[0];
            if (resolvedIDs.Length == 0) return new TreeIDPartialNoResolutionError(id);
            return new TreeIDPartialAmbiguousResolutionError(id, resolvedIDs);
        }

        public Task<Errorable<TreeID>[]> ResolvePartialIDs(params TreeID.Partial[] ids)
        {
            return TaskEx.WhenAll(ids.SelectAsArray(id => ResolvePartialID(id)));
        }
    }
}
