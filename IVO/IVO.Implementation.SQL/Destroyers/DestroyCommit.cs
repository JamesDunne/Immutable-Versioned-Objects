using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Errors;

namespace IVO.Implementation.SQL.Persists
{
    public sealed class DestroyCommit : IDataOperation<Errorable<CommitID>>
    {
        private CommitID _id;

        public DestroyCommit(CommitID id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Commit.Single();
            var cmdText = String.Format(
                @"DELETE FROM {0} WHERE [{1}] = @{1}",
                Tables.TableName_Commit,
                pkName
            );

            var cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@" + pkName, new SqlBinary((byte[])_id));
            return cmd;
        }

        public Errorable<CommitID> Return(SqlCommand cmd, int rowsAffected)
        {
            return this._id;
        }
    }
}
