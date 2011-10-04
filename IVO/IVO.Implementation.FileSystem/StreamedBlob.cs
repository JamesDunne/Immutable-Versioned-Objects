using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Errors;
using System.IO;
using IVO.Definition;

namespace IVO.Implementation.FileSystem
{
    internal sealed class StreamedBlob : IStreamedBlob
    {
        private StreamedBlobRepository blrepo;

        internal StreamedBlob(StreamedBlobRepository blrepo, BlobID id, long? length = null)
        {
            this.blrepo = blrepo;
            this.ID = id;
            this.Length = length;

#if DEBUG
            // Catch the error early-on.
            if (!blrepo.FileSystem.getPathByID(id).Exists)
                throw new BlobIDRecordDoesNotExistError(id);
#endif
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

        public async Task<Errorable<TResult>> ReadStreamAsync<TResult>(Func<System.IO.Stream, Task<Errorable<TResult>>> read) where TResult : class
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            if (!path.Exists) return new BlobIDRecordDoesNotExistError(this.ID);

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (SHA1StreamReader hasher = new SHA1StreamReader(sr))
            {
                var result = await read(hasher).ConfigureAwait(continueOnCapturedContext: false);

                return validateHash(hasher, result);
            }
        }

        public async Task<Errorable> ReadStreamAsync(Func<System.IO.Stream, Task<Errorable>> read)
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            if (!path.Exists) return new BlobIDRecordDoesNotExistError(this.ID);

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (SHA1StreamReader hasher = new SHA1StreamReader(sr))
            {
                var result = await read(hasher).ConfigureAwait(continueOnCapturedContext: false);

                return validateHash(hasher, result);
            }
        }

        public Errorable<TResult> ReadStream<TResult>(Func<Stream, Errorable<TResult>> read) where TResult : class
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            if (!path.Exists) return new BlobIDRecordDoesNotExistError(this.ID);

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (SHA1StreamReader hasher = new SHA1StreamReader(sr))
            {
                var result = read(hasher);

                return validateHash(hasher, result);
            }
        }

        public Errorable ReadStream(Func<Stream, Errorable> read)
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            if (!path.Exists) return new BlobIDRecordDoesNotExistError(this.ID);

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (SHA1StreamReader hasher = new SHA1StreamReader(sr))
            {
                var result = read(hasher);

                return validateHash(hasher, result);
            }
        }
    }
}
