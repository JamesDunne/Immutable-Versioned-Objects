using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Asynq;
using System.Data;
using System.Data.SqlClient;

namespace IVO.TestSupport
{
    public sealed class ClearAllDataNonQuery : IDataOperation<int>
    {
        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            // WARNING: this deletes ALL data from ALL tables in the target database!
            var cmd = new SqlCommand(
@"-- disable all constraints
EXEC sp_msforeachtable ""ALTER TABLE ? NOCHECK CONSTRAINT all""

-- delete data in all tables
EXEC sp_MSForEachTable ""DELETE FROM ?""

-- enable all constraints
EXEC sp_msforeachtable ""ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all""
",
                cn
            );

            return cmd;
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }

        public int Return(SqlCommand cmd, int rowsAffected)
        {
            return rowsAffected;
        }
    }
}
