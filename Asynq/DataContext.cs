using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asynq
{
    /// <summary>
    /// Represents the context used for data queries.
    /// </summary>
    public sealed class DataContext
    {
        private string connectionString;

        private DataContext(SqlConnectionStringBuilder connStringBuilder)
        {
            // TODO: is it necessary to force this here?
            connStringBuilder.AsynchronousProcessing = true;

            this.connectionString = connStringBuilder.ConnectionString;
        }

        /// <summary>
        /// Constructs a new context using the given connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public DataContext(string connectionString)
            : this(new SqlConnectionStringBuilder(connectionString))
        {
        }

        /// <summary>
        /// Asynchronously execute the given query expected to return 0 or more rows.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="expectedCapacity"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public async Task<List<T>> ExecuteListQueryAsync<T>(ISimpleDataQuery<T> query, int expectedCapacity = 10, TaskFactory<List<T>> factory = null)
        {
            if (expectedCapacity < 0) expectedCapacity = 0;
            if (factory == null) factory = new TaskFactory<List<T>>();

            using (var cn = new SqlConnection(this.connectionString))
            {
                var cmd = query.ConstructCommand(cn);
                cn.Open();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection | query.GetCustomCommandBehaviors(cn, cmd)))
                {
                    // Build up the result list:
                    List<T> results = new List<T>(expectedCapacity);
                    while (dr.Read())
                    {
                        var row = query.Project(cmd, dr);
                        results.Add(row);
                    }

                    return results;
                }
            }
        }

        /// <summary>
        /// Asynchronously execute the given query expected to return 0 or more rows.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="expectedCapacity"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public async Task<T> ExecuteListQueryAsync<T>(IComplexDataQuery<T> query, int expectedCapacity = 10, TaskFactory<T> factory = null)
        {
            if (expectedCapacity < 0) expectedCapacity = 0;
            if (factory == null) factory = new TaskFactory<T>();

            using (var cn = new SqlConnection(this.connectionString))
            {
                var cmd = query.ConstructCommand(cn);
                cn.Open();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection | query.GetCustomCommandBehaviors(cn, cmd)))
                {
                    return await query.RetrieveAsync(cmd, dr, expectedCapacity);
                }
            }
        }

        /// <summary>
        /// Asynchronously execute the given query expected to return 0 or 1 items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public async Task<T> ExecuteSingleQueryAsync<T>(ISimpleDataQuery<T> query, TaskFactory<T> factory = null)
        {
            if (factory == null) factory = new TaskFactory<T>();

            using (var cn = new SqlConnection(this.connectionString))
            {
                var cmd = query.ConstructCommand(cn);
                cn.Open();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection | CommandBehavior.SingleRow | query.GetCustomCommandBehaviors(cn, cmd)))
                {
                    // If no row read, return the default:
                    if (!dr.Read()) return default(T);

                    var row = query.Project(cmd, dr);

                    return row;
                }
            }
        }

        /// <summary>
        /// Asynchronously execute the given query expected to return 0 or 1 items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public async Task<T> ExecuteSingleQueryAsync<T>(IComplexDataQuery<T> query, TaskFactory<T> factory = null)
        {
            if (factory == null) factory = new TaskFactory<T>();

            using (var cn = new SqlConnection(this.connectionString))
            {
                var cmd = query.ConstructCommand(cn);
                cn.Open();

                using (SqlDataReader dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection | query.GetCustomCommandBehaviors(cn, cmd)))
                {
                    T row = await query.RetrieveAsync(cmd, dr);

                    return row;
                }
            }
        }
        
        /// <summary>
        /// Asynchronously execute the given data operation expected to not return any values.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public async Task<T> ExecuteNonQueryAsync<T>(IDataOperation<T> op, TaskFactory<T> factory = null)
        {
            if (factory == null) factory = new TaskFactory<T>();

            using (var cn = new SqlConnection(this.connectionString))
            {
                var cmd = op.ConstructCommand(cn);
                cn.Open();

                int rc = await cmd.ExecuteNonQueryAsync();

                return op.Return(cmd, rc);
            }
        }

        #region Synchronous

        /// <summary>
        /// Synchronously execute the given query expected to return 0 or 1 items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public T ExecuteSingleQuery<T>(IComplexDataQuery<T> query)
        {
            using (var cn = new SqlConnection(this.connectionString))
            {
                var cmd = query.ConstructCommand(cn);
                cn.Open();

                using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection | query.GetCustomCommandBehaviors(cn, cmd)))
                {
                    T row = query.Retrieve(cmd, dr);

                    return row;
                }
            }
        }

        #endregion
    }
}
