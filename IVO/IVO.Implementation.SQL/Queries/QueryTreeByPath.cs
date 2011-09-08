using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Exceptions;
using System.Data;

namespace IVO.Implementation.SQL.Queries
{
    public class QueryTreeByPath : IComplexDataQuery<Tuple<TreeID, ImmutableContainer<TreeID, Tree>>>
    {
        private TreeID _rootid;
        private CanonicalTreePath _path;

        public QueryTreeByPath(TreeID rootid, CanonicalTreePath path)
        {
            this._rootid = rootid;
            this._path = path;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Tree.Single();
            string cmdText =
                // First, find the @treeid by path from the root TreeID:
@";WITH rec AS (
    SELECT      CONVERT(binary(20), NULL) AS treeid, tr.treeid AS linked_treeid, CONVERT(nvarchar(128), NULL) COLLATE SQL_Latin1_General_CP1_CS_AS AS name, CONVERT(NVARCHAR(256), N'/') COLLATE SQL_Latin1_General_CP1_CS_AS AS [path]
    FROM        [dbo].[Tree] tr
    WHERE       tr.treeid = @rootid
    UNION ALL
    SELECT      tr.treeid, tr.linked_treeid, tr.name, CONVERT(NVARCHAR(256), parent.[path] + tr.name + N'/') COLLATE SQL_Latin1_General_CP1_CS_AS AS [path]
    FROM        [dbo].[TreeTree] tr
    JOIN        rec parent ON parent.linked_treeid = tr.treeid
)
SELECT  @treeid = [tr].linked_treeid FROM rec tr WHERE tr.[path] = @path;" +
                // Next, query the Tree recursively from there:
@";WITH Trees AS (
    SELECT      CONVERT(binary(20), NULL) AS treeid, tr.treeid AS linked_treeid, CONVERT(nvarchar(128), NULL) COLLATE SQL_Latin1_General_CP1_CS_AS AS name
    FROM        [dbo].[Tree] tr
    WHERE       tr.treeid = @treeid
    UNION ALL
    SELECT      tr.treeid, tr.linked_treeid, tr.name
    FROM        [dbo].[TreeTree] tr
    JOIN        Trees parent ON parent.linked_treeid = tr.treeid
)
SELECT  [tr].[treeid] AS tr_treeid
       ,[tr].[linked_treeid] AS tr_linked_treeid
       ,[tr].[name] AS tr_name
       ,[bl].[linked_blobid] AS trbl_linked_blobid
       ,[bl].[name] AS trbl_name
FROM    Trees tr
LEFT JOIN [dbo].[TreeBlob] bl ON bl.treeid = tr.linked_treeid";

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@rootid", new SqlBinary((byte[])this._rootid));
            cmd.AddInParameter("@path", new SqlString(this._path.ToString()));
            cmd.AddOutParameter("@treeid", System.Data.SqlDbType.Binary, 20);
            return cmd;
        }

        public Tuple<TreeID, ImmutableContainer<TreeID, Tree>> Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCount)
        {
            Dictionary<TreeID, Tree.Builder> trees = new Dictionary<TreeID, Tree.Builder>(expectedCount);

            TreeID? root = null;

            // Iterate through rows of the recursive query, assuming ordering of rows guarantees tree depth locality.
            while (dr.Read())
            {
                SqlBinary btreeid = dr.GetSqlBinary(0);
                SqlBinary blinked_treeid = dr.GetSqlBinary(1);
                SqlString treename = dr.GetSqlString(2);
                SqlBinary linked_blobid = dr.GetSqlBinary(3);
                SqlString blobname = dr.GetSqlString(4);

                TreeID? treeid = btreeid.IsNull ? (TreeID?)null : (TreeID)btreeid.Value;
                TreeID? linked_treeid = blinked_treeid.IsNull ? (TreeID?)null : (TreeID)blinked_treeid.Value;

                TreeID pullFor = treeid.HasValue ? treeid.Value : linked_treeid.Value;

                // Use the first row as the root:
                if (!root.HasValue) root = linked_treeid.Value;

                Tree.Builder curr;
                if (!trees.TryGetValue(pullFor, out curr))
                {
                    curr = new Tree.Builder(new List<TreeTreeReference>(), new List<TreeBlobReference>());
                    trees.Add(pullFor, curr);
                }

                // The tree to add the blob link to:
                Tree.Builder blobTree = curr;

                // Add a tree link:
                if (treeid.HasValue && linked_treeid.HasValue)
                {
                    // Create the Tree.Builder for the linked_treeid if it does not exist:
                    if (!trees.TryGetValue(linked_treeid.Value, out blobTree))
                    {
                        blobTree = new Tree.Builder(new List<TreeTreeReference>(), new List<TreeBlobReference>());
                        trees.Add(linked_treeid.Value, blobTree);
                    }

                    List<TreeTreeReference> treeRefs = curr.Trees;

                    bool isDupe = false;
                    if (treeRefs.Count > 0)
                    {
                        // Only check the previous ref record for dupe:
                        // TODO: verify that SQL Server will *always* return rows in an order that supports depth locality from a recursive CTE.
                        isDupe = (
                            (treeRefs[treeRefs.Count - 1].TreeID == linked_treeid.Value) &&
                            (treeRefs[treeRefs.Count - 1].Name == treename.Value)
                        );
                    }

                    // Don't re-add the same tree link:
                    if (!isDupe)
                        treeRefs.Add(new TreeTreeReference.Builder(treename.Value, linked_treeid.Value));
                }

                // Add a blob link to the child or parent tree:
                if (!linked_blobid.IsNull)
                    blobTree.Blobs.Add(new TreeBlobReference.Builder(blobname.Value, (BlobID)linked_blobid.Value));
            }

            // If no root assigned, then no tree retrieved:
            if (!root.HasValue) return null;

            // Return the final result with immutable objects:
            return new Tuple<TreeID, ImmutableContainer<TreeID, Tree>>(
                root.Value,
                new ImmutableContainer<TreeID, Tree>(
                    tr => tr.ID,
                    trees.Select(kv =>
                        // Verify that the retrieved ID is equivalent to the constructed ID:
                        ((Tree)kv.Value).Assert(tr => kv.Key == tr.ID, tr => new TreeIDMismatchException(tr.ID, kv.Key))
                    )
                )
            );
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
