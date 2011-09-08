using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Exceptions;
using System.Data;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryBlob : ISimpleDataQuery<Blob>
    {
        private BlobID _id;

        public QueryBlob(BlobID id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
                @"SELECT {0} FROM {1}{2}{3} WHERE blobid = @blobid",
                Tables.TablePKs_Blob.Concat(Tables.ColumnNames_Blob).NameCommaList(),
                Tables.TableName_Blob,
                "", // no alias
                Tables.TableFromHint_Blob
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@blobid", new SqlBinary((byte[])_id));
            return cmd;
        }

        public Blob Project(SqlCommand cmd, SqlDataReader dr)
        {
            BlobID id = (BlobID) dr.GetSqlBinary(0).Value;

            Blob.Builder b = new Blob.Builder(
                pContents:  dr.GetSqlBinary(1).Value
            );

            Blob bl = b;
            if (bl.ID != id) throw new BlobIDMismatchException(bl.ID, id);

            return bl;
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
