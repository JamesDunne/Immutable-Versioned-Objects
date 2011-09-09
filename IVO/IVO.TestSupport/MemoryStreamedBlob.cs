using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Models;
using System.Threading.Tasks;

namespace IVO.TestSupport
{
    public sealed class MemoryStreamedBlob : IStreamedBlob
    {
        private byte[] buf;

        public MemoryStreamedBlob(string contents)
        {
            this.buf = Encoding.UTF8.GetBytes(contents);

            using (var ms = new System.IO.MemoryStream(buf))
                this.ID = StreamedBlobMethods.ComputeID(ms);
        }

        public BlobID ID { get; private set; }

        public Task<TResult> ReadStream<TResult>(Func<System.IO.Stream, TResult> read)
        {
            return TaskEx.Run(() =>
            {
                using (var ms = new System.IO.MemoryStream(buf))
                    return read(ms);
            });
        }

        public Task ReadStream(Action<System.IO.Stream> read)
        {
            return TaskEx.Run(() =>
            {
                using (var ms = new System.IO.MemoryStream(buf))
                    read(ms);
            });
        }

        public long? Length { get { return buf.LongLength; } }
    }
}
