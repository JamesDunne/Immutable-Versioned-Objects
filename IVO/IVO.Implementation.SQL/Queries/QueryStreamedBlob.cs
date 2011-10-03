using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Errors;
using System.Data;
using System.Threading.Tasks;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryStreamedBlob<TResult> : IComplexDataQuery<TResult>
    {
        private BlobID _id;
        private Func<System.IO.Stream, Task<TResult>> readAsync;
        private Func<System.IO.Stream, TResult> read;

        public QueryStreamedBlob(BlobID id, Func<System.IO.Stream, Task<TResult>> readAsync)
        {
            this._id = id;
            this.readAsync = readAsync;
        }

        public QueryStreamedBlob(BlobID id, Func<System.IO.Stream, TResult> read)
        {
            this._id = id;
            this.read = read;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
                @"SELECT [blobid], DATALENGTH([contents]) AS [length], [contents] FROM {1}{2}{3} WHERE blobid = @blobid",
                Tables.TablePKs_Blob.Concat(Tables.ColumnNames_Blob).NameCommaList(),
                Tables.TableName_Blob,
                "", // no alias
                Tables.TableFromHint_Blob
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@blobid", new SqlBinary((byte[])_id));
            return cmd;
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            // NOTE: This is critical for streaming content from the database:
            return CommandBehavior.SequentialAccess;
        }
        
        public async Task<TResult> RetrieveAsync(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            if (!dr.Read()) return default(TResult);

            // Read the BlobID:
            BlobID id = (BlobID) dr.GetSqlBinary(0).Value.ToArray(20);
            if (id != this._id) throw new ComputedBlobIDMismatchError();

            // Read the length of the contents:
            long datalength = dr.GetSqlInt64(1).Value;

            // Use the lambda to read from the contents stream:
            TResult result = await this.readAsync(new BlobReaderStream(dr, 2, length: datalength));
            return result;
        }

        public TResult Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            if (!dr.Read()) return default(TResult);

            // Read the BlobID:
            BlobID id = (BlobID)dr.GetSqlBinary(0).Value.ToArray(20);
            if (id != this._id) throw new ComputedBlobIDMismatchError();

            // Read the length of the contents:
            long datalength = dr.GetSqlInt64(1).Value;

            // Use the lambda to read from the contents stream:
            TResult result = this.read(new BlobReaderStream(dr, 2, length: datalength));
            return result;
        }
    }
}
