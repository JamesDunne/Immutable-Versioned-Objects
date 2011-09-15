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

        public async Task<TResult> ReadStream<TResult>(Func<System.IO.Stream, Task<TResult>> read)
        {
            using (var ms = new System.IO.MemoryStream(buf))
                return await read(ms);
        }

        public async Task ReadStream(Func<System.IO.Stream, Task> read)
        {
            using (var ms = new System.IO.MemoryStream(buf))
                await read(ms);
        }

        public long? Length { get { return buf.LongLength; } }
    }
}
