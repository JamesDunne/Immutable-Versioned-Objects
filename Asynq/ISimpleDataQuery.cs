using System.Data.SqlClient;
using System.Data;

namespace Asynq
{
    /// <summary>
    /// Represents a simple (i.e. single- or multi-record list via direct row-to-record relation) query.
    /// </summary>
    /// <typeparam name="T">The type of object to return results with.</typeparam>
    public interface ISimpleDataQuery<T>
    {
        /// <summary>
        /// Construct a SqlCommand to be executed with the given SqlConnection.
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        SqlCommand ConstructCommand(SqlConnection cn);

        /// <summary>
        /// Get a set of custom CommandBehaviors used to execute the given command on the connection.
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd);

        /// <summary>
        /// Project a row from the SqlDataReader to an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="cmd">The executed command, useful for obtaining output parameter values.</param>
        /// <param name="dr">The DataReader obtained by executing the command.</param>
        /// <remarks>
        /// Implementation MUST NOT call Read() or NextResult() on the DataReader; this will break the
        /// interface contract. To support these complex scenarios, implement IComplexDataQuery instead.
        /// </remarks>
        /// <returns></returns>
        T Project(SqlCommand cmd, SqlDataReader dr);
    }
}
