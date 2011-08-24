using System.Data.SqlClient;
using System.Collections.Generic;

namespace Asynq
{
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
        /// <param name="dr"></param>
        /// <returns></returns>
        T Retrieve(SqlDataReader dr, int expectedCapacity = 10);
    }
}
