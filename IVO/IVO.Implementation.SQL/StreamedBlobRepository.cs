using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asynq;
using IVO.Implementation.SQL;
using IVO.Implementation.SQL.Persists;
using IVO.Implementation.SQL.Queries;
using IVO.Definition;
using IVO.Definition.Models;
using IVO.Definition.Containers;
using IVO.Definition.Repositories;
using System.Collections.Concurrent;
using IVO.Definition.Errors;
using System.Data.SqlClient;

namespace IVO.Implementation.SQL
{
    public sealed class StreamedBlobRepository : IStreamedBlobRepository
    {
        private DataContext db;

        internal DataContext DB { get { return db; } }

        public StreamedBlobRepository(DataContext db)
        {
            this.db = db;
        }

        private sealed class BlobIDUpdate : IDataOperation<int>
        {
            private byte[] oldpk;
            private byte[] newpk;

            public BlobIDUpdate(byte[] oldpk, byte[] newpk)
            {
                this.oldpk = oldpk;
                this.newpk = newpk.ToArray(21);
            }

            public SqlCommand ConstructCommand(SqlConnection cn)
            {
                var cmd = cn.CreateCommand();
                // This could be the same blobid as an existing record, so check if an existing blobid exists
                // and if so, delete the temporary blob just uploaded.
                cmd.CommandText = String.Format(
@"IF (EXISTS(SELECT 1 FROM {0} WITH (UPDLOCK) WHERE {1} = @newpk)) BEGIN
  DELETE FROM {0} WHERE {1} = @pk;
END ELSE BEGIN
  UPDATE {0} SET {1} = @newpk WHERE {1} = @pk;
END",
                    Tables.TableName_Blob,
                    Tables.TablePKs_Blob.Single()
                );
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Parameters.Add("@pk", System.Data.SqlDbType.Binary, 21).Value = oldpk;
                cmd.Parameters.Add("@newpk", System.Data.SqlDbType.Binary, 21).Value = newpk;
                return cmd;
            }

            public int Return(SqlCommand cmd, int rowsAffected)
            {
                return rowsAffected;
            }
        }

        public async Task<Errorable<IStreamedBlob>> PersistBlob(PersistingBlob blob)
        {
            SqlCommand insert, update;
            
            insert = new SqlCommand(@"INSERT INTO [dbo].[Blob] ([blobid], [contents]) VALUES (@pk, @chunk)");
            insert.Parameters.Add("@pk", System.Data.SqlDbType.Binary, 21);
            insert.Parameters.Add("@chunk", System.Data.SqlDbType.Binary);

            update = new SqlCommand(@"UPDATE [dbo].[Blob] SET [contents].WRITE(@chunk, NULL, NULL) WHERE [blobid] = @pk");
            update.Parameters.Add("@pk", System.Data.SqlDbType.Binary, 21);
            update.Parameters.Add("@chunk", System.Data.SqlDbType.Binary);

            // Create a random dummy ID for uploading:
            // FIXME: we need guaranteed uniqueness!
            byte[] dummyID = new byte[21];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
                rng.GetBytes(dummyID);
            dummyID[20] = 0xFF;

            // Create a BlobWriter to INSERT and UPDATE to the database in chunks:
            var bw = new BlobWriter(db, insert, update, dummyID);

            // Create a SHA1 calculating wrapper around the input stream:
            var sha1Reader = new SHA1StreamReader(blob.Stream);
            
            // Asynchronously upload the blob:
            await bw.UploadAsync(sha1Reader);

            // Get the hash and use that as the BlobID:
            byte[] blobHash = sha1Reader.GetHash();

            // Now update the Blob record with the new BlobID:
            await db.ExecuteNonQueryAsync(new BlobIDUpdate(dummyID, blobHash));

            // Return the StreamedBlob instance that can read from the uploaded blob record:
            return new StreamedBlob(this, new BlobID(blobHash), bw.Length);
        }

        public async Task<Errorable<IStreamedBlob>[]> PersistBlobs(params PersistingBlob[] blobs)
        {
            // Start persisting blobs:
            var tasks = new Task<Errorable<IStreamedBlob>>[blobs.Length];
            for (int i = 0; i < blobs.Length; ++i)
            {
                tasks[i] = PersistBlob(blobs[i]);
            }

            // When all persists are complete, roll up the results from all the tasks into a single array:
            var streamedBlobs = await Task.WhenAll(tasks);

            return streamedBlobs;
        }

        public Task<Errorable<BlobID>> DeleteBlob(BlobID id)
        {
            return db.ExecuteNonQueryAsync(new DestroyBlob(id));
        }

        public Task<Errorable<BlobID>[]> DeleteBlobs(params BlobID[] ids)
        {
            var tasks = new Task<Errorable<BlobID>>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                tasks[i] = DeleteBlob(ids[i]);

            return Task.WhenAll(tasks);
        }

        public Task<Errorable<IStreamedBlob>> GetBlob(BlobID id)
        {
            return Task.FromResult((Errorable<IStreamedBlob>)new StreamedBlob(this, id));
        }

        public Task<Errorable<IStreamedBlob>[]> GetBlobs(params BlobID[] ids)
        {
            var blobs = new Errorable<IStreamedBlob>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                blobs[i] = new StreamedBlob(this, ids[i]);
            return Task.FromResult(blobs);
        }

        public async Task<Errorable<BlobID>> ResolvePartialID(BlobID.Partial id)
        {
            var resolvedIDs = await db.ExecuteListQueryAsync(new ResolvePartialBlobID(id));
            if (resolvedIDs.Length == 1) return resolvedIDs[0];
            if (resolvedIDs.Length == 0) return new BlobIDPartialNoResolutionError(id);
            return new BlobIDPartialAmbiguousResolutionError(id, resolvedIDs);
        }

        public Task<Errorable<BlobID>[]> ResolvePartialIDs(params BlobID.Partial[] ids)
        {
            return Task.WhenAll(ids.SelectAsArray(id => ResolvePartialID(id)));
        }
    }
}
