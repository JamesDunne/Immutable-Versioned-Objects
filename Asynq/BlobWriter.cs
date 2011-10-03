using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.IO;

namespace Asynq
{
    public sealed class BlobWriter
    {
        private DataContext db;
        private SqlCommand cmdInsert;
        private SqlCommand cmdUpdate;
        private object pkValue;

        public BlobWriter(DataContext db, SqlCommand insert, SqlCommand update, object pkValue)
        {
            if (db == null) throw new ArgumentNullException("db");
            if (insert == null) throw new ArgumentNullException("insert");
            if (update == null) throw new ArgumentNullException("update");

            if (!insert.Parameters.Contains("@chunk")) throw new ArgumentException("Insert query must define a @chunk parameter", "insert");
            if (!update.Parameters.Contains("@chunk")) throw new ArgumentException("Update query must define a @chunk parameter", "update");
            if (!update.Parameters.Contains("@pk")) throw new ArgumentException("Update query must define a @pk parameter", "update");
            if ((pkValue == null) && !insert.Parameters.Contains("@pk")) throw new ArgumentException("Insert query must define a @pk parameter if pkValue is null", "pkValue");

            this.db = db;
            this.cmdInsert = insert;
            this.cmdUpdate = update;
            this.pkValue = pkValue;
            this.Length = 0L;
        }

        private sealed class InsertOperation : IDataOperation<object>
        {
            private SqlCommand insert;
            private byte[] chunk;
            
            public InsertOperation(SqlCommand insert, byte[] chunk, object pkValue)
            {
                this.insert = insert;
                this.chunk = chunk;
                if (pkValue != null)
                    insert.Parameters["@pk"].Value = pkValue;
            }

            public SqlCommand ConstructCommand(SqlConnection cn)
            {
                insert.Connection = cn;
                var prmChunk = insert.Parameters["@chunk"];
                prmChunk.Size = chunk.Length;
                prmChunk.Value = chunk;
                return insert;
            }

            public object Return(SqlCommand cmd, int rowsAffected)
            {
                if (insert.Parameters.Contains("@pk")) return insert.Parameters["@pk"].Value;
                return null;
            }
        }

        private sealed class UpdateOperation : IDataOperation<int>
        {
            private SqlCommand update;
            private object pkValue;
            private byte[] chunk;

            public UpdateOperation(SqlCommand update, object pkValue)
            {
                this.update = update;
                this.pkValue = pkValue;

                update.Parameters["@pk"].Value = pkValue;
            }

            public void SetChunk(byte[] chunk)
            {
                this.chunk = chunk;
            }

            public SqlCommand ConstructCommand(SqlConnection cn)
            {
                update.Connection = cn;

                var prmChunk = update.Parameters["@chunk"];
                prmChunk.Size = chunk.Length;
                prmChunk.Value = chunk;
                
                return update;
            }

            public int Return(SqlCommand cmd, int rowsAffected)
            {
                return rowsAffected;
            }
        }

        public long Length { get; private set; }

        public async Task UploadAsync(Stream input)
        {
            const int bufferSize = 8040;
            byte[] buf = new byte[bufferSize];
            byte[] chunk = buf;
            int nr;

            // Create a buffer around the input so we're guaranteed to read complete chunk sizes:
            using (var bs = new BufferedStream(input, bufferSize))
            {
                // Perform the first read:
                nr = await bs.ReadAsync(buf, 0, bufferSize);
                //if (nr == 0) return;

                // Did we read less than bufferSize?
                if (nr < bufferSize)
                {
                    chunk = new byte[nr];
                    Array.Copy(buf, chunk, nr);
                }

                // Perform the INSERT first:
                object pk = await db.ExecuteNonQueryAsync(new InsertOperation(cmdInsert, chunk, pkValue));
                this.Length += nr;

                // Early out if we're done:
                if (nr < bufferSize) return;

                // Use the INSERT query's @pk parameter or fall back to the passed-in value:
                pk = pk ?? pkValue;

                // UPDATE while there is more to be uploaded:
                var update = new UpdateOperation(cmdUpdate, pk);

                while (nr == bufferSize)
                {
                    nr = await bs.ReadAsync(buf, 0, bufferSize);
                    // This shouldn't happen:
                    if (nr == 0) break;

                    // Crop the chunk array down to the number of bytes actually read:
                    if (nr < bufferSize)
                    {
                        chunk = new byte[nr];
                        Array.Copy(buf, chunk, nr);
                    }

                    // Perform the UPDATE:
                    update.SetChunk(chunk);
                    await db.ExecuteNonQueryAsync(update);
                    this.Length += nr;
                }
            }
        }
    }
}
