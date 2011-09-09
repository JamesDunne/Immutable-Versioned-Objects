﻿using System;
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
    public class QueryTreeByPath : IComplexDataQuery<Tree>
    {
        private TreeTreePath _path;

        public QueryTreeByPath(TreeTreePath path)
        {
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
@"SELECT tr.name, tr.linked_treeid FROM [dbo].[TreeTree] tr WHERE [{0}] = @treeid;
SELECT bl.name, bl.linked_blobid FROM [dbo].[TreeBlob] bl WHERE [{0}] = @treeid;";

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@rootid", new SqlBinary((byte[])this._path.RootTreeID));
            cmd.AddInParameter("@path", new SqlString(this._path.Path.ToString()));
            cmd.AddOutParameter("@treeid", System.Data.SqlDbType.Binary, 20);
            return cmd;
        }

        public Tree Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            Tree.Builder tb = new Tree.Builder(new List<TreeTreeReference>(), new List<TreeBlobReference>());

            // Read the TreeTreeReferences:
            while (dr.Read())
            {
                var name = dr.GetSqlString(0).Value;
                var linked_treeid = (TreeID)dr.GetSqlBinary(1).Value;

                tb.Trees.Add(new TreeTreeReference.Builder(name, linked_treeid));
            }

            if (!dr.NextResult()) return null;

            // Read the TreeBlobReferences:
            while (dr.Read())
            {
                var name = dr.GetSqlString(0).Value;
                var linked_blobid = (BlobID)dr.GetSqlBinary(1).Value;

                tb.Blobs.Add(new TreeBlobReference.Builder(name, linked_blobid));
            }

            Tree tr = tb;
            TreeID retrievedId = (TreeID)((SqlBinary)cmd.Parameters["@treeid"].SqlValue).Value;
            if (tr.ID != retrievedId) throw new TreeIDMismatchException(tr.ID, retrievedId);

            return tr;
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
