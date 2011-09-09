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

        public Task<TResult> ReadStream<TResult>(Func<System.IO.Stream, TResult> read)
        {
            return TaskEx.Run(() =>
            {
                FileInfo path = blrepo.getPathByID(this.ID);

                // Or throw exception?
                if (!path.Exists) return default(TResult);

                // Open the file for reading and send it to the lambda function:
                using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    return read(sr);
            });
        }

        public Task ReadStream(Action<System.IO.Stream> read)
        {
            return TaskEx.Run(() =>
            {
                FileInfo path = blrepo.getPathByID(this.ID);

                // Or throw exception?
                if (!path.Exists) return;

                // Open the file for reading and send it to the lambda function:
                using (FileStream sr = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    read(sr);
            });
        }
    }
}
