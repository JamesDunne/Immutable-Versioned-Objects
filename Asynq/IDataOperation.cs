using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Asynq
{
    /// <summary>
    /// Represents a state-changing operation to the data store.
    /// </summary>
    /// <typeparam name="TResult">Type of result object to return upon completion.</typeparam>
    public interface IDataOperation<TResult>
    {
        /// <summary>
        /// Construct a SqlCommand to be executed with the given SqlConnection.
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        SqlCommand ConstructCommand(SqlConnection cn);

        /// <summary>
        /// Returns a result through the ExecuteNonQueryAsync method that invoked this data operation.
        /// </summary>
        /// <param name="cmd">The executed command, useful for obtaining output parameter values.</param>
        /// <param name="rowsAffected">The return value from (End)ExecuteNonQuery representing the number of rows affected by the query.</param>
        /// <returns></returns>
        TResult Return(SqlCommand cmd, int rowsAffected);
    }
}
