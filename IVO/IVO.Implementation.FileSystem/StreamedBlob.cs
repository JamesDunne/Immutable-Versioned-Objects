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
        private BlobRepository blrepo;

        internal StreamedBlob(BlobRepository blrepo, BlobID id, long? length = null)
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
                string idStr = ID.ToString();
                string path = System.IO.Path.Combine(blrepo.FileSystem.Root.FullName, "objects", idStr.Substring(0, 2), idStr.Substring(2));

                // Or throw exception?
                if (!File.Exists(path)) return default(TResult);

                // Open the file for reading and send it to the lambda function:
                using (FileStream sr = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    return read(sr);
            });
        }

        public Task ReadStream(Action<System.IO.Stream> read)
        {
            return TaskEx.Run(() =>
            {
                string idStr = ID.ToString();
                string path = System.IO.Path.Combine(blrepo.FileSystem.Root.FullName, "objects", idStr.Substring(0, 2), idStr.Substring(2));

                // Or throw exception?
                if (!File.Exists(path)) return;

                // Open the file for reading and send it to the lambda function:
                using (FileStream sr = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    read(sr);
            });
        }
    }
}
