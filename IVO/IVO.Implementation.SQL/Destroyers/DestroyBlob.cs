using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Errors;

namespace IVO.Implementation.SQL.Persists
{
    public sealed class DestroyBlob : IDataOperation<Errorable<BlobID>>
    {
        private BlobID _id;

        public DestroyBlob(BlobID id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Blob.Single();
            var cmdText = String.Format(
                @"DELETE FROM {0} WHERE [{1}] = @{1}",
                Tables.TableName_Blob,
                pkName
            );

            var cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@" + pkName, new SqlBinary((byte[])_id));
            return cmd;
        }

        public Errorable<BlobID> Return(SqlCommand cmd, int rowsAffected)
        {
            return this._id;
        }
    }
}
