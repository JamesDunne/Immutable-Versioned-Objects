using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Asynq;
using System.Data;
using System.Data.SqlClient;

namespace IVO.TestSupport
{
    public sealed class SetSingleUserNonQuery : IDataOperation<int>
    {
        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            var cmd = new SqlCommand(
                String.Format(
@"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;",
                    cn.Database
                ),
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
