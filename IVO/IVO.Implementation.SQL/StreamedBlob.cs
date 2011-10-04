using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Models;
using System.Threading.Tasks;
using IVO.Implementation.SQL.Queries;
using IVO.Definition.Errors;
using IVO.Definition;

namespace IVO.Implementation.SQL
{
    internal sealed class StreamedBlob : IStreamedBlob
    {
        private StreamedBlobRepository blrepo;

        internal StreamedBlob(StreamedBlobRepository blrepo, BlobID id, long? length = null)
        {
            this.ID = id;
            this.blrepo = blrepo;
            this.Length = length;
        }

        public BlobID ID { get; private set; }
        public long? Length { get; private set; }

        private const int dummyBufferSize = 4096;

        private Errorable<TResult> validateHash<TResult>(SHA1StreamReader hasher, Errorable<TResult> result)
        {
            // Read the rest of the stream in order to fully compute the SHA-1 hash:
            byte[] dummyBuffer = new byte[dummyBufferSize];
            while (hasher.Read(dummyBuffer, 0, dummyBufferSize) > 0) ;

            // Verify the computed BlobID is equivalent to the expected BlobID to detect data tampering:
            BlobID hashID = new BlobID(hasher.GetHash());
            if (hashID != this.ID) return new ComputedBlobIDMismatchError(hashID, this.ID);

            return result;
        }

        private Errorable validateHash(SHA1StreamReader hasher, Errorable result)
        {
            // Read the rest of the stream in order to fully compute the SHA-1 hash:
            byte[] dummyBuffer = new byte[dummyBufferSize];
            while (hasher.Read(dummyBuffer, 0, dummyBufferSize) > 0) ;

            // Verify the computed BlobID is equivalent to the expected BlobID to detect data tampering:
            BlobID hashID = new BlobID(hasher.GetHash());
            if (hashID != this.ID) return new ComputedBlobIDMismatchError(hashID, this.ID);

            return result;
        }

        public Task<Errorable<TResult>> ReadStreamAsync<TResult>(Func<System.IO.Stream, Task<Errorable<TResult>>> read) where TResult : class
        {
            return blrepo.DB.ExecuteSingleQueryAsync(new QueryStreamedBlob<Errorable<TResult>>(this.ID, async sr =>
            {
                using (SHA1StreamReader hasher = new SHA1StreamReader(sr))
                {
                    var result = await read(hasher).ConfigureAwait(continueOnCapturedContext: false);

                    return validateHash(hasher, result);
                }
            }));
        }

        public Task<Errorable> ReadStreamAsync(Func<System.IO.Stream, Task<Errorable>> read)
        {
            return blrepo.DB.ExecuteSingleQueryAsync(new QueryStreamedBlob<Errorable>(this.ID, async sr =>
            {
                using (SHA1StreamReader hasher = new SHA1StreamReader(sr))
                {
                    var result = await read(hasher).ConfigureAwait(continueOnCapturedContext: false);

                    return validateHash(hasher, result);
                }
            }));
        }

        public Errorable<TResult> ReadStream<TResult>(Func<System.IO.Stream, Errorable<TResult>> read) where TResult : class
        {
            return blrepo.DB.ExecuteSingleQuery(new QueryStreamedBlob<Errorable<TResult>>(this.ID, sr =>
            {
                using (SHA1StreamReader hasher = new SHA1StreamReader(sr))
                {
                    var result = read(hasher);

                    return validateHash(hasher, result);
                }
            }));
        }

        public Errorable ReadStream(Func<System.IO.Stream, Errorable> read)
        {
            return blrepo.DB.ExecuteSingleQuery(new QueryStreamedBlob<Errorable>(this.ID, sr =>
            {
                using (SHA1StreamReader hasher = new SHA1StreamReader(sr))
                {
                    var result = read(hasher);

                    return validateHash(hasher, result);
                }
            }));
        }
    }
}
