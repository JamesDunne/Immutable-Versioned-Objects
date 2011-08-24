using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Asynq
{
    public interface IDataOperation
    {
        /// <summary>
        /// Construct a SqlCommand to be executed with the given SqlConnection.
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        SqlCommand ConstructCommand(SqlConnection cn);
    }
}
