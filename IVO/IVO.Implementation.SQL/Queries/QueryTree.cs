using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using System.Collections.Generic;
using System.Data;
using IVO.Definition.Exceptions;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryTree : IComplexDataQuery<Tree>
    {
        private TreeID _id;

        public QueryTree(TreeID id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Tree.Single();
            string cmdText = String.Format(
@"SELECT tr.name, tr.linked_treeid FROM [dbo].[TreeTree] tr WHERE [{0}] = @treeid;
SELECT bl.name, bl.linked_blobid FROM [dbo].[TreeBlob] bl WHERE [{0}] = @treeid;",
                pkName,
                Tables.TableName_Tree,
                Tables.TableFromHint_Tree
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@treeid", new SqlBinary((byte[])this._id));
            return cmd;
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
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
            if (tr.ID != _id) throw new TreeIDMismatchException(tr.ID, _id);

            return tr;
        }
    }
}
