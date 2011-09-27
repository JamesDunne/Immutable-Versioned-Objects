using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Models;
using System.Threading.Tasks;
using IVO.Definition.Errors;

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

        public long? Length { get { return buf.LongLength; } }

        public async Task<Errorable<TResult>> ReadStreamAsync<TResult>(Func<System.IO.Stream, Task<Errorable<TResult>>> read) where TResult : class
        {
            using (var ms = new System.IO.MemoryStream(buf))
                return await read(ms);
        }

        public async Task<Errorable> ReadStreamAsync(Func<System.IO.Stream, Task<Errorable>> read)
        {
            using (var ms = new System.IO.MemoryStream(buf))
                return await read(ms);
        }

        public Errorable<TResult> ReadStream<TResult>(Func<System.IO.Stream, Errorable<TResult>> read) where TResult : class
        {
            using (var ms = new System.IO.MemoryStream(buf))
                return read(ms);
        }

        public Errorable ReadStream(Func<System.IO.Stream, Errorable> read)
        {
            using (var ms = new System.IO.MemoryStream(buf))
                return read(ms);
        }
    }
}
