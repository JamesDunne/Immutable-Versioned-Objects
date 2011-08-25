using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Asynq
{
    public interface IDataOperation<TResult>
    {
        /// <summary>
        /// Construct a SqlCommand to be executed with the given SqlConnection.
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        SqlCommand ConstructCommand(SqlConnection cn);

        /// <summary>
        /// Returns a result through the AsynqNonQuery method that invoked this data operation.
        /// </summary>
        /// <param name="rowsAffected"></param>
        /// <returns></returns>
        TResult Return(int rowsAffected);
    }
}
