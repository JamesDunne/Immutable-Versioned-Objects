using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Errors;
using System.IO;

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
                throw new BlobIDRecordDoesNotExistError();
#endif
        }

        public BlobID ID { get; private set; }
        public long? Length { get; private set; }

        public async Task<Errorable<TResult>> ReadStreamAsync<TResult>(Func<System.IO.Stream, Task<Errorable<TResult>>> read) where TResult : class
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            if (!path.Exists) return new BlobIDRecordDoesNotExistError();

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                return await read(sr).ConfigureAwait(continueOnCapturedContext: false);
        }

        public async Task<Errorable> ReadStreamAsync(Func<System.IO.Stream, Task<Errorable>> read)
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            if (!path.Exists) return new BlobIDRecordDoesNotExistError();

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                return await read(sr).ConfigureAwait(continueOnCapturedContext: false);
        }

        public Errorable<TResult> ReadStream<TResult>(Func<Stream, Errorable<TResult>> read) where TResult : class
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            if (!path.Exists) return new BlobIDRecordDoesNotExistError();

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                return read(sr);
        }

        public Errorable ReadStream(Func<Stream, Errorable> read)
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            if (!path.Exists) return new BlobIDRecordDoesNotExistError();

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                return read(sr);
        }
    }
}
