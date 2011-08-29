using System.Data.SqlClient;
using System.Collections.Generic;

namespace Asynq
{
    /// <summary>
    /// Represents a complex (i.e. custom DataReader implementation) query against the data store.
    /// </summary>
    /// <typeparam name="T">The type of object to return results with.</typeparam>
    public interface IComplexDataQuery<T>
    {
        /// <summary>
        /// Construct a SqlCommand to be executed with the given SqlConnection.
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        SqlCommand ConstructCommand(SqlConnection cn);

        /// <summary>
        /// Read all rows from the SqlDataReader to an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="cmd">The executed command, useful for obtaining output parameter values.</param>
        /// <param name="dr">The DataReader obtained by executing the command.</param>
        /// <remarks>
        /// Implementation must call Read() on the DataReader and may also use NextResult() if multiple
        /// result-sets are queried. For simplistic row-to-record mapping, implement ISimpleDataQuery
        /// instead.
        /// </remarks>
        /// <returns></returns>
        T Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10);
    }
}
