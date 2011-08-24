using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;
using System.Collections.Generic;
using GitCMS.Definition.Containers;

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
            throw new NotImplementedException();

            Dictionary<TreeID, Tree> trees = new Dictionary<TreeID, Tree>();

            while (dr.Read())
            {
                dr.GetSqlBinary(0);
            }

            return new Tuple<TreeID, TreeContainer>(this._id, new TreeContainer(trees));
        }
    }
}
