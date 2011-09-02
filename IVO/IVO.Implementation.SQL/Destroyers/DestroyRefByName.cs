using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;

namespace IVO.Implementation.SQL.Persists
{
    public sealed class DestroyRefByName : IDataOperation<int>
    {
        private string _refName;

        public DestroyRefByName(string refName)
        {
            this._refName = refName;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            var cmdText = String.Format(
                @"DELETE FROM {0} WHERE [name] = @refname;",
                Tables.TableName_Ref
            );

            var cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@refname", new SqlString(_refName));
            return cmd;
        }

        public int Return(SqlCommand cmd, int rowsAffected)
        {
            return rowsAffected;
        }
    }
}
