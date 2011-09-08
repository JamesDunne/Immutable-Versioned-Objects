using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Exceptions;

namespace IVO.Implementation.SQL.Queries
{
    public class QueryBlobByPath : IComplexDataQuery<TreePathBlob>
    {
        private TreeID _rootid;
        private CanonicalBlobPath _path;

        public QueryBlobByPath(TreeID rootid, CanonicalBlobPath path)
        {
            this._rootid = rootid;
            this._path = path;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Tree.Single();
            string cmdText =
@";WITH rec AS (
    SELECT      CONVERT(binary(20), NULL) AS treeid, tr.treeid AS linked_treeid, CONVERT(nvarchar(128), NULL) COLLATE SQL_Latin1_General_CP1_CS_AS AS name, CONVERT(NVARCHAR(256), N'/') COLLATE SQL_Latin1_General_CP1_CS_AS AS [path]
    FROM        [dbo].[Tree] tr
    WHERE       tr.treeid = @rootid
    UNION ALL
    SELECT      tr.treeid, tr.linked_treeid, tr.name, CONVERT(NVARCHAR(256), parent.[path] + tr.name + N'/') COLLATE SQL_Latin1_General_CP1_CS_AS AS [path]
    FROM        [dbo].[TreeTree] tr
    JOIN        rec parent ON parent.linked_treeid = tr.treeid
)
SELECT  bl.[blobid], bl.[contents]
FROM rec tr
JOIN [dbo].[TreeBlob] trbl ON trbl.treeid = tr.linked_treeid
JOIN [dbo].[Blob] bl ON bl.blobid = trbl.linked_blobid
WHERE tr.[path] + trbl.name = @path;";

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@rootid", new SqlBinary((byte[])this._rootid));
            cmd.AddInParameter("@path", new SqlString(this._path.ToString()));
            return cmd;
        }

        public TreePathBlob Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCount)
        {
            if (!dr.Read()) return null;

            BlobID id = (BlobID)dr.GetSqlBinary(0).Value;

            Blob.Builder b = new Blob.Builder(
                pContents: dr.GetSqlBinary(1).Value
            );

            Blob bl = b;
            if (bl.ID != id) throw new BlobIDMismatchException(bl.ID, id);

            return new TreePathBlob(this._rootid, this._path, bl);
        }
    }
}
