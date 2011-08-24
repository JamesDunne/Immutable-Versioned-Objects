using System.Data.SqlClient;

namespace Asynq
{
    public interface ISimpleDataQuery<T>
    {
        /// <summary>
        /// Construct a SqlCommand to be executed with the given SqlConnection.
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        SqlCommand ConstructCommand(SqlConnection cn);

        /// <summary>
        /// Project a row from the SqlDataReader to an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        T Project(SqlDataReader dr);
    }
}
