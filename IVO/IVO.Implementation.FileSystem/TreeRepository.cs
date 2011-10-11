using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Containers;
using IVO.Definition.Errors;
using IVO.Definition.Models;
using IVO.Definition.Repositories;

namespace IVO.Implementation.FileSystem
{
    public sealed class TreeRepository : ITreeRepository
    {
        private FileSystem system;

        public TreeRepository(FileSystem system)
        {
            this.system = system;
        }

        #region Private details

        private async Task<Errorable<TreeNode>> getTree(TreeID id)
        {
            FileInfo fi = system.getPathByID(id);
            if (!fi.Exists) return new TreeIDRecordDoesNotExistError(id);

            byte[] buf;
            int nr = 0;
            using (var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true))
            {
                // TODO: implement an async buffered Stream:
                buf = new byte[16384];
                nr = await fs.ReadAsync(buf, 0, 16384).ConfigureAwait(continueOnCapturedContext: false);
                if (nr >= 16384)
                {
                    // My, what a large tree you have!
                    throw new NotSupportedException();
                }
            }

            TreeNode.Builder tb = new TreeNode.Builder(new List<TreeTreeReference>(), new List<TreeBlobReference>());

            // Parse the Tree:
            using (var ms = new MemoryStream(buf, 0, nr, false))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("tree "))
                    {
                        string linked_treeid = line.Substring(5, (TreeID.ByteArrayLength * 2));
                        string name = line.Substring(6 + (TreeID.ByteArrayLength * 2));

                        // Attempt to parse the TreeID and verify its existence:
                        Errorable<TreeID> trid = TreeID.TryParse(linked_treeid);
                        if (trid.HasErrors) return trid.Errors;
                        if (!system.getPathByID(trid.Value).Exists) return new TreeIDRecordDoesNotExistError(trid.Value);
                        tb.Trees.Add(new TreeTreeReference.Builder(name, trid.Value));
                    }
                    else if (line.StartsWith("blob "))
                    {
                        string linked_blobid = line.Substring(5, (TreeID.ByteArrayLength * 2));
                        string name = line.Substring(6 + (TreeID.ByteArrayLength * 2));

                        // Attempt to parse the BlobID and verify its existence:
                        Errorable<BlobID> blid = BlobID.TryParse(linked_blobid);
                        if (blid.HasErrors) return blid.Errors;
                        if (!system.getPathByID(blid.Value).Exists) return new BlobIDRecordDoesNotExistError(blid.Value);
                        tb.Blobs.Add(new TreeBlobReference.Builder(name, blid.Value));
                    }
                }
            }

            // Create the immutable Tree from the Builder:
            TreeNode tr = tb;
            // Validate the computed TreeID:
            if (tr.ID != id) return new ComputedTreeIDMismatchError(tr.ID, id);

            return tr;
        }

        private Errorable<TreeNode> persistTree(TreeNode tr)
        {
            Debug.Assert(tr != null);

            // Check that all referenced blobs are already persisted:
            foreach (var trbl in tr.Blobs)
            {
                if (!system.getPathByID(trbl.BlobID).Exists)
                    return new BlobIDRecordDoesNotExistError(trbl.BlobID);
            }

            // Check that all referenced blobs are already persisted:
            foreach (var trtr in tr.Trees)
            {
                if (!system.getPathByID(trtr.TreeID).Exists)
                    return new TreeIDRecordDoesNotExistError(trtr.TreeID);
            }

            FileInfo fi = system.getPathByID(tr.ID);
            lock (FileSystem.SystemLock)
            {
                fi.Refresh();
                // TODO: maybe a TreeIDRecordAlreadyExistsError?
                if (fi.Exists) return tr;

                // Create directory if it doesn't exist:
                if (!fi.Directory.Exists)
                {
                    Debug.WriteLine(String.Format("New DIR '{0}'", fi.Directory.FullName));
                    fi.Directory.Create();
                }

                // Write the tree contents to the file:
                using (var fs = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    Debug.WriteLine(String.Format("New TREE '{0}'", fi.FullName));
                    tr.WriteTo(fs);
                }
            }

            return tr;
        }

        private void deleteTree(TreeID id)
        {
            FileInfo fi = system.getPathByID(id);
            lock (FileSystem.SystemLock)
            {
                if (!fi.Exists) return;

                fi.Delete();
            }
        }

        private async Task<Errorable<TreeNode[]>> getTreeRecursively(TreeID id)
        {
            var eroot = await getTree(id).ConfigureAwait(continueOnCapturedContext: false);
            if (eroot.HasErrors) return eroot.Errors;

            var root = eroot.Value;
            var rootArr = new TreeNode[1] { root };

            if (root.Trees.Length == 0)
                return rootArr;

            Task<Errorable<TreeNode[]>>[] tasks = new Task<Errorable<TreeNode[]>>[root.Trees.Length];
            for (int i = 0; i < root.Trees.Length; ++i)
            {
                tasks[i] = getTreeRecursively(root.Trees[i].TreeID);
            }

            // Await all the tree retrievals:
            var allTrees = await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);

            // Roll up all the errors:
            ErrorContainer errors =
                (
                    from etrs in allTrees
                    where etrs.HasErrors
                    select etrs.Errors
                ).Aggregate(new ErrorContainer(), (acc, err) => acc + err);

            if (errors.HasAny) return errors;

            // Flatten out the tree arrays:
            var flattened =
                from etrs in allTrees
                from tr in etrs.Value
                select tr;

            // Return the final array:
            return rootArr.Concat(flattened).ToArray(allTrees.Sum(ta => ta.Value.Length) + 1);
        }

        private async Task<Errorable<TreeIDPathMapping>> getTreeIDByPath(TreeTreePath path)
        {
            // Get the root Tree:
            var eroot = await getTree(path.RootTreeID).ConfigureAwait(continueOnCapturedContext: false);
            if (eroot.HasErrors) return eroot.Errors;

            TreeNode root = eroot.Value;

            return await getTreeIDByPath(root, path);
        }

        private async Task<Errorable<TreeIDPathMapping>> getTreeIDByPath(TreeNode root, TreeTreePath path)
        {
            ReadOnlyCollection<string> parts = path.Path.Parts;
            if (parts.Count == 0) return new TreeIDPathMapping(path, path.RootTreeID);

            int j = 0;

            // Start descending into child nodes:
            TreeNode node = root, nextNode;
            while (node != null)
            {
                nextNode = null;

                // Check all child nodes against the next name in the path:
                for (int i = 0; i < node.Trees.Length; ++i)
                    // TODO: specific string comparison logic!
                    if (parts[j] == node.Trees[i].Name)
                    {
                        TreeID nextID = node.Trees[i].TreeID;

                        // Run out of path components? This is it!
                        if (++j == parts.Count) return new TreeIDPathMapping(path, (TreeID?)nextID);

                        // Load up the next node so we can scan through its child nodes:
                        var enextNode = await getTree(nextID).ConfigureAwait(continueOnCapturedContext: false);
                        if (enextNode.HasErrors) return enextNode.Errors;

                        nextNode = enextNode.Value;
                        break;
                    }

                // Attempt to continue:
                node = nextNode;
            }

            // If we got here it means that we didn't find what we were looking for:
            return new TreeIDPathMapping(path, (TreeID?)null);
        }

        #endregion

        public async Task<Errorable<TreeNode>> PersistTree(TreeID rootid, ImmutableContainer<TreeID, TreeNode> trees)
        {
            if (trees == null) throw new ArgumentNullException("trees");
            // TODO: better return value than `null`
            if (trees.Count == 0) return (TreeNode)null;

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
            HashSet<TreeID> isPersisting = new HashSet<TreeID>();

            int lastDepth = reverseDepthOrder.Peek().depth;
            foreach (var curr in reverseDepthOrder)
            {
                Debug.WriteLine(String.Format("{0}: {1}", curr.depth, curr.id.ToString(firstLength: 7)));
                // An invariant of the algorithm, enforced via assert:
                Debug.Assert(curr.depth <= lastDepth);

                // Did we move to the next depth group:
                if (curr.depth != lastDepth)
                {
                    Debug.WriteLine(String.Format("Awaiting depth group {0}...", lastDepth));
                    // Wait for the last depth group to finish persisting:
                    await Task.WhenAll(persistTasks);
                    // TODO: roll up errors!

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
                var tsk = Task.Run(() => persistTree(node));

                // Add the task to the depth group to await:
                Debug.WriteLine(String.Format("Adding to depth group {0}...", curr.depth));
                persistTasks.Add(tsk);

                // Keep track of the last depth level:
                lastDepth = curr.depth;
            }

            // The final depth group should be depth 0 with at most 1 element: the root node.
            Debug.Assert(lastDepth == 0);
            if (persistTasks.Count > 0)
            {
                // Await the last group (the root node):
                Debug.WriteLine(String.Format("Awaiting depth group {0}...", lastDepth));
                await Task.WhenAll(persistTasks);
            }

            // Return the root TreeNode:
            return persistTasks[0].Result;
        }

        public Task<Errorable<TreeNode>> GetTree(TreeID id)
        {
            return getTree(id);
        }

        public Task<Errorable<TreeNode>[]> GetTrees(params TreeID[] ids)
        {
            Task<Errorable<TreeNode>>[] tasks = new Task<Errorable<TreeNode>>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                TreeID id = ids[i];
                tasks[i] = getTree(id);
            }
            return Task.WhenAll(tasks);
        }

        public Task<Errorable<TreeIDPathMapping>> GetTreeIDByPath(TreeTreePath path)
        {
            return getTreeIDByPath(path);
        }

        public Task<Errorable<TreeIDPathMapping>[]> GetTreeIDsByPaths(params TreeTreePath[] paths)
        {
            return Task.WhenAll(paths.SelectAsArray(path => getTreeIDByPath(path)));
        }

        public async Task<Errorable<TreeID>> DeleteTreeRecursively(TreeID rootid)
        {
            var etrees = await getTreeRecursively(rootid).ConfigureAwait(continueOnCapturedContext: false);
            if (etrees.HasErrors) return etrees.Errors;

            var trees = etrees.Value;

            // TODO: test that 'tr' is captured properly in the lambda.
            Task[] tasks = trees.SelectAsArray(tr => Task.Run(() => deleteTree(tr.ID)));

            await Task.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);

            return rootid;
        }

        public async Task<Errorable<TreeTree>> GetTreeRecursively(TreeID rootid)
        {
            // Get all trees recursively:
            var eall = await getTreeRecursively(rootid).ConfigureAwait(continueOnCapturedContext: false);
            if (eall.HasErrors) return eall.Errors;

            TreeNode[] all = eall.Value;

            // Return them (all[0] is the root):
            return new TreeTree(all[0].ID, new ImmutableContainer<TreeID, TreeNode>(tr => tr.ID, all));
        }

        public async Task<Errorable<TreeTree>> GetTreeRecursivelyFromPath(TreeTreePath path)
        {
            // Find the TreeID given the path and its root TreeID:
            var etpm = await getTreeIDByPath(path).ConfigureAwait(continueOnCapturedContext: false);
            if (etpm.HasErrors) return etpm.Errors;

            TreeIDPathMapping tpm = etpm.Value;
            // TODO: TreePathNotFoundError
            if (!tpm.TreeID.HasValue) return (TreeTree)null;

            // Now use GetTreeRecursively to do the rest:
            return await GetTreeRecursively(tpm.TreeID.Value).ConfigureAwait(continueOnCapturedContext: false);
        }

        public Task<Errorable<TreeID>> ResolvePartialID(TreeID.Partial id)
        {
            FileInfo[] fis = system.getPathsByPartialID(id);
            if (fis.Length == 1) return Task.FromResult(TreeID.TryParse(id.ToString().Substring(0, 2) + fis[0].Name));
            if (fis.Length == 0) return Task.FromResult((Errorable<TreeID>)new TreeIDPartialNoResolutionError(id));
            return Task.FromResult((Errorable<TreeID>)new TreeIDPartialAmbiguousResolutionError(id, fis.SelectAsArray(f => TreeID.TryParse(id.ToString().Substring(0, 2) + f.Name).Value)));
        }

        public Task<Errorable<TreeID>[]> ResolvePartialIDs(params TreeID.Partial[] ids)
        {
            return Task.WhenAll(ids.SelectAsArray(id => ResolvePartialID(id)));
        }

        public async Task<Errorable<TreeNode[]>> GetTreeNodesAlongPath(TreeTreePath path)
        {
            // TODO: test me!
            List<TreeNode> trnodes = new List<TreeNode>(path.Path.Parts.Count + 1);
            Errorable<TreeNode> etr;

            etr = await getTree(path.RootTreeID);
            if (etr.HasErrors) return etr.Errors;
            trnodes.Add(etr.Value);

            for (int i = 0; i < path.Path.Parts.Count; ++i)
            {
                TreeTreeReference trrf = etr.Value.Trees.SingleOrDefault(tr => tr.Name == path.Path.Parts[i]);
                if (trrf == null) break;

                etr = await getTree(trrf.TreeID);
                if (etr.HasErrors) return etr.Errors;

                trnodes.Add(etr.Value);
            }

            return trnodes.ToArray();
        }

        public async Task<Errorable<TreeTree>> PersistTreeNodesByBlobPaths(Maybe<TreeID> rootID, IEnumerable<CanonicalBlobIDPath> paths)
        {
            TreeNode root;
            if (rootID.HasValue)
            {
                var eroot = await getTree(rootID.Value);
                if (eroot.HasErrors) return eroot.Errors;
                root = eroot.Value;
            }
            else root = null;

            var nodeByPath = new Dictionary<string, Tuple<CanonicalTreePath, TreeNode.Builder>>();

            int depthCapacity = 5;
            var depthGroups = new List<Tuple<CanonicalTreePath, TreeNode.Builder>>[depthCapacity];

            Tuple<CanonicalTreePath, TreeNode.Builder> tpl;

            // Add the root node by default:
            CanonicalTreePath rootPath = (CanonicalTreePath)"/";
            
            if (rootID.HasValue)
                tpl = new Tuple<CanonicalTreePath, TreeNode.Builder>(rootPath, new TreeNode.Builder(root));
            else
                tpl = new Tuple<CanonicalTreePath, TreeNode.Builder>(rootPath, new TreeNode.Builder());
            nodeByPath.Add(rootPath.ToString(), tpl);

            // Initialize the depthGroups array:
            int deepest = 0;
            depthGroups[0] = new List<Tuple<CanonicalTreePath, TreeNode.Builder>>(1) { tpl };
            for (int i = 1; i < depthCapacity; ++i) depthGroups[i] = new List<Tuple<CanonicalTreePath, TreeNode.Builder>>();

            Debug.WriteLine(String.Empty);
            foreach (CanonicalBlobIDPath path in paths)
            {
                TreeNode.Builder tnb;
                CanonicalTreePath treePath;
                string treePathStr;

                // Create node builders for each subfolder in the path below the root:
                for (int depth = 1; depth <= path.Path.Tree.Parts.Count; ++depth)
                {
                    treePath = path.Path.Tree.GetPartialTree(depth);
                    treePathStr = treePath.ToString();

                    // Get the node builder for the current path or create it:
                    if (!nodeByPath.ContainsKey(treePathStr))
                    {
                        tnb = null;

                        if (rootID.HasValue)
                        {
                            // Get the TreeNode if available, otherwise construct a new builder:
                            var ecurr = await getTreeIDByPath(root, new TreeTreePath(rootID.Value, treePath));
                            if (ecurr.HasErrors) return ecurr.Errors;
                            var curr = ecurr.Value;

                            if (curr.TreeID.HasValue)
                            {
                                // Get the TreeNode:
                                var etr = await getTree(curr.TreeID.Value);
                                if (etr.HasErrors) return etr.Errors;

                                // Create the builder from that node:
                                tnb = new TreeNode.Builder(etr.Value);
                            }
                        }

                        if (tnb == null)
                        {
                            // New builder:
                            tnb = new TreeNode.Builder(new List<TreeTreeReference>(), new List<TreeBlobReference>());
                        }

                        // Keep track of this node builder for the given path:
                        Debug.WriteLine(treePathStr);
                        tpl = new Tuple<CanonicalTreePath, TreeNode.Builder>(treePath, tnb);
                        nodeByPath.Add(treePathStr, tpl);

                        // Maintain the depthGroups array:
                        if (depth >= depthCapacity)
                        {
                            // TODO: tune this resize policy:
                            int newCapacity = depth + 2;

                            // Extend the array:
                            Array.Resize(ref depthGroups, newCapacity);

                            // Initialize all the new elements:
                            for (int i = depthCapacity; i < newCapacity; ++i)
                                depthGroups[i] = new List<Tuple<CanonicalTreePath, TreeNode.Builder>>();

                            depthCapacity = newCapacity;
                        }

                        if (depth > deepest) deepest = depth;
                        depthGroups[depth].Add(tpl);
                    }
                }

                treePath = path.Path.Tree;
                treePathStr = treePath.ToString();

                // Get the node builder for the current path or create it:
                Tuple<CanonicalTreePath, TreeNode.Builder> ptnb;
                bool test = nodeByPath.TryGetValue(treePathStr, out ptnb);
                Debug.Assert(test);

                // Add or update the TreeBlobReference for this blob path:
                string blobName = path.Path.Name;
                var trblb = new TreeBlobReference.Builder(blobName, path.BlobID);

                int blidx = ptnb.Item2.Blobs.FindIndex(trbl => trbl.Name == blobName);
                if (blidx == -1)
                    ptnb.Item2.Blobs.Add(trblb);
                else
                    ptnb.Item2.Blobs[blidx] = trblb;
            }

#if DEBUG
            Debug.WriteLine(String.Empty);
            foreach (var mtpl in nodeByPath.Values)
            {
                Debug.WriteLine(String.Format(
                    "{0}: {1}",
                    mtpl.Item1.ToString(),
                    String.Join(", ", mtpl.Item2.Blobs.Select(trbl => trbl.Name + ":" + trbl.BlobID.ToString(firstLength: 7)))
                ));
            }
#endif

            for (int i = deepest; i >= 0; --i)
            {
                Debug.WriteLine(String.Empty);
                Debug.WriteLine("Depth #{0}", i);
                var nodes = depthGroups[i];
                foreach (var node in nodes)
                {
                    Debug.WriteLine(node.Item1.ToString());
                    foreach (var bl in node.Item2.Blobs)
                    {
                        Debug.WriteLine(new string('_', i * 2) + bl.Name + ":" + bl.BlobID.ToString(firstLength: 7));
                    }
                }
            }

            // Persist each tree depth level:
            var awaiting = new List<Task<Errorable<TreeNode>>>(depthGroups[deepest].Count);
            var lastNodes = new List<Tuple<CanonicalTreePath, TreeNode>>(depthGroups[deepest].Count);
            var result = new Dictionary<TreeID, TreeNode>();

            Debug.WriteLine(String.Format("Starting depth group #{0}", deepest));
            foreach (var dnode in depthGroups[deepest])
            {
                // Finalize the TreeNode:
                TreeNode tn = dnode.Item2;
                lastNodes.Add(new Tuple<CanonicalTreePath, TreeNode>(dnode.Item1, tn));

                if (!result.ContainsKey(tn.ID))
                {
                    Debug.WriteLine(String.Format("{0}: Persisting TreeID {1}", dnode.Item1.ToString(), tn.ID.ToString(firstLength: 7)));
                    result.Add(tn.ID, tn);

                    // Start persistence task and add to `awaiting`
                    var tsk = Task.Run(() => persistTree(tn));
                    awaiting.Add(tsk);
                }
                else
                {
                    Debug.WriteLine(String.Format("{0}: Already persisted TreeID {1}", dnode.Item1.ToString(), tn.ID.ToString(firstLength: 7)));
                }
            }

            for (int i = deepest - 1; i >= 0; --i)
            {
                // Await last depth group if non-empty:
                if (awaiting.Count > 0)
                {
                    Debug.WriteLine(String.Format("Awaiting previous depth group's persistence"));
                    var errs = await Task.WhenAll(awaiting);
                    if (errs.Any(err => err.HasErrors))
                        return errs.Aggregate(new ErrorContainer(), (acc, err) => acc + err.Errors);
                }
                Debug.WriteLine(String.Format("Starting depth group #{0}", i));

                awaiting = new List<Task<Errorable<TreeNode>>>(depthGroups[i].Count);
                var currNodes = new List<Tuple<CanonicalTreePath, TreeNode>>(depthGroups[i].Count);

                // Update each tree node in this depth group:
                foreach (var node in depthGroups[i])
                {
                    // Create TreeTreeReferences to point to child TreeIDs:
                    var childNodes = lastNodes.Where(t => t.Item1.GetParent() == node.Item1);
                    foreach (var childNode in childNodes)
                    {
                        // Add or update the TreeTreeReference to the child tree:
                        int tridx = node.Item2.Trees.FindIndex(trtr => trtr.Name == childNode.Item1.Name);

                        var trtrb = new TreeTreeReference.Builder(childNode.Item1.Name, childNode.Item2.ID);
                        if (tridx == -1)
                        {
                            Debug.WriteLine(String.Format("{0}: Adding (name: {1}, treeid: {2})", node.Item1.ToString(), trtrb.Name, trtrb.TreeID.ToString(firstLength: 7)));
                            node.Item2.Trees.Add(trtrb);
                        }
                        else
                        {
                            Debug.WriteLine(String.Format("{0}: Updating (name: {1}, old-treeid: {2}, new-treeid: {3})", node.Item1.ToString(), trtrb.Name, node.Item2.Trees[tridx].TreeID, trtrb.TreeID.ToString(firstLength: 7)));
                            node.Item2.Trees[tridx] = trtrb;
                        }
                    }

                    // Finalize the TreeNode:
                    TreeNode tn = (TreeNode)node.Item2;
                    currNodes.Add(new Tuple<CanonicalTreePath, TreeNode>(node.Item1, tn));
                    if (!result.ContainsKey(tn.ID))
                    {
                        Debug.WriteLine(String.Format("{0}: Persisting TreeID {1}", node.Item1.ToString(), tn.ID.ToString(firstLength: 7)));
                        result.Add(tn.ID, tn);

                        // Start persistence task and add to `awaiting`
                        var tsk = Task.Run(() => persistTree(tn));
                        awaiting.Add(tsk);
                    }
                    else
                    {
                        Debug.WriteLine(String.Format("{0}: Already persisted TreeID {1}", node.Item1.ToString(), tn.ID.ToString(firstLength: 7)));
                    }
                }

                lastNodes = currNodes;
            }

            if (awaiting.Count > 0)
            {
                Debug.WriteLine(String.Format("Awaiting previous depth group's persistence"));
                var errs = await Task.WhenAll(awaiting);
                if (errs.Any(err => err.HasErrors))
                    return errs.Aggregate(new ErrorContainer(), (acc, err) => acc + err.Errors);
            }

            // Last depth group should be count of 1, the new root TreeNode:
            Debug.Assert(lastNodes.Count == 1);

            return new TreeTree(lastNodes[0].Item2.ID, new ImmutableContainer<TreeID,TreeNode>(result));
        }
    }
}
