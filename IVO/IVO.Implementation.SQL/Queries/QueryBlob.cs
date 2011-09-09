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
    public sealed class QueryBlob : ISimpleDataQuery<IStreamedBlob>
    {
        private BlobID _id;
        private BlobRepository _blrepo;

        public QueryBlob(BlobRepository blrepo, BlobID id)
        {
            this._blrepo = blrepo;
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
                @"SELECT [blobid], DATALENGTH([contents]) AS [length] FROM {1}{2}{3} WHERE blobid = @blobid",
                Tables.TablePKs_Blob.Concat(Tables.ColumnNames_Blob).NameCommaList(),
                Tables.TableName_Blob,
                "", // no alias
                Tables.TableFromHint_Blob
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@blobid", new SqlBinary((byte[])_id));
            return cmd;
        }

        public IStreamedBlob Project(SqlCommand cmd, SqlDataReader dr)
        {
            BlobID id = (BlobID) dr.GetSqlBinary(0).Value;
            long length = dr.GetSqlInt64(1).Value;

            return new StreamedBlob(this._blrepo, id, length);
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
