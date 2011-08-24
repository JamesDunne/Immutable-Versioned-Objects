using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;
using System.Collections.Generic;
using GitCMS.Definition.Containers;
using System.Diagnostics;

namespace GitCMS.Data.Queries
{
    public class QueryTreeRecursively : IComplexDataQuery<Tuple<TreeID, TreeContainer>>
    {
        private TreeID _id;

        public QueryTreeRecursively(TreeID id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Tree.Single();
            string cmdText =
@";WITH Trees AS (
    SELECT      CONVERT(binary(20), NULL) AS treeid, tr.treeid AS linked_treeid, CONVERT(nvarchar(128), NULL) AS name
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
            cmd.AddInParameter("@treeid", new SqlBinary((byte[])this._id));
            return cmd;
        }

        public Tuple<TreeID, TreeContainer> Retrieve(SqlDataReader dr, int expectedCount)
        {
            Dictionary<TreeID, Tree.Builder> trees = new Dictionary<TreeID, Tree.Builder>(expectedCount);

            while (dr.Read())
            {
                SqlBinary btreeid = dr.GetSqlBinary(0);
                SqlBinary blinked_treeid = dr.GetSqlBinary(1);
                SqlString treename = dr.GetSqlString(2);
                SqlBinary linked_blobid = dr.GetSqlBinary(3);
                SqlString blobname = dr.GetSqlString(4);

                TreeID? treeid = btreeid.IsNull ? (TreeID?)null : (TreeID)btreeid.Value;
                TreeID? linked_treeid = blinked_treeid.IsNull ? (TreeID?)null : (TreeID)blinked_treeid.Value;

                Tree.Builder curr;

                if (!treeid.HasValue)
                {
                    Debug.Assert(linked_treeid.HasValue);
                    if (!trees.TryGetValue(linked_treeid.Value, out curr))
                    {
                        curr = new Tree.Builder(new List<TreeTreeReference>(), new List<TreeBlobReference>());
                        trees.Add(linked_treeid.Value, curr);
                    }
                }
                else
                {
                    if (!trees.TryGetValue(treeid.Value, out curr))
                    {
                        curr = new Tree.Builder(new List<TreeTreeReference>(), new List<TreeBlobReference>());
                        trees.Add(treeid.Value, curr);
                    }
                }

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
                        isDupe = (
                            (treeRefs[treeRefs.Count - 1].TreeID == linked_treeid.Value) &&
                            (treeRefs[treeRefs.Count - 1].Name == treename.Value)
                        );
                    }

                    // Don't re-add the same tree link:
                    if (!isDupe)
                    {
                        treeRefs.Add(new TreeTreeReference.Builder(treename.Value, linked_treeid.Value));
                    }
                }

                // Add a blob link to the child or parent tree:
                if (!linked_blobid.IsNull)
                {
                    blobTree.Blobs.Add(new TreeBlobReference.Builder(blobname.Value, (BlobID)linked_blobid.Value));
                }
            }

            return new Tuple<TreeID, TreeContainer>(this._id, new TreeContainer(trees.Values.Select(t => (Tree)t)));
        }
    }
}
