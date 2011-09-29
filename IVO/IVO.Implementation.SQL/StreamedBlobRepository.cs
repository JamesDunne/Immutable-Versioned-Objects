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
            new Random().NextBytes(dummyID);
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
            // TODO!

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
            var streamedBlobs = await TaskEx.WhenAll(tasks);

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

            return TaskEx.WhenAll(tasks);
        }

        public Task<Errorable<IStreamedBlob>> GetBlob(BlobID id)
        {
            return TaskEx.FromResult((Errorable<IStreamedBlob>)new StreamedBlob(this, id));
        }

        public Task<Errorable<IStreamedBlob>[]> GetBlobs(params BlobID[] ids)
        {
            var blobs = new Errorable<IStreamedBlob>[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
                blobs[i] = new StreamedBlob(this, ids[i]);
            return TaskEx.FromResult(blobs);
        }
    }
}
