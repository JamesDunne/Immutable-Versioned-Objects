using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
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
        }

        public BlobID ID { get; private set; }
        public long? Length { get; private set; }

        public async Task<TResult> ReadStreamAsync<TResult>(Func<System.IO.Stream, Task<TResult>> read)
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            // Or throw exception?
            if (!path.Exists) return default(TResult);

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                return await read(sr);
        }

        public async Task ReadStreamAsync(Func<System.IO.Stream, Task> read)
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            // Or throw exception?
            if (!path.Exists) return;

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                await read(sr);
        }

        public TResult ReadStream<TResult>(Func<Stream, TResult> read)
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            // Or throw exception?
            if (!path.Exists) return default(TResult);

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                return read(sr);
        }

        public void ReadStream(Action<Stream> read)
        {
            FileInfo path = blrepo.FileSystem.getPathByID(this.ID);

            // Or throw exception?
            if (!path.Exists) return;

            // Open the file for reading and send it to the lambda function:
            using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                read(sr);
        }
    }
}
