using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace Asynq
{
    /// <summary>
    /// An abstract class that represents a data query to be constructed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class DataQuery<T>
    {
        /// <summary>
        /// Construct a SqlCommand to be executed with the given SqlConnection.
        /// </summary>
        /// <param name="cn"></param>
        /// <returns></returns>
        public abstract SqlCommand ConstructCommand(SqlConnection cn);

        /// <summary>
        /// Project a row from the SqlDataReader to an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public abstract T Project(SqlDataReader dr);
    }
}
