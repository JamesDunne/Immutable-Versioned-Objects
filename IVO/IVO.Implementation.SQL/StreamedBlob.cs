using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Models;
using System.Threading.Tasks;
using IVO.Implementation.SQL.Queries;

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

        public Task<TResult> ReadStreamAsync<TResult>(Func<System.IO.Stream, Task<TResult>> read)
        {
            return blrepo.DB.ExecuteSingleQueryAsync(new QueryStreamedBlob<TResult>(this.ID, read));
        }

        public Task ReadStreamAsync(Func<System.IO.Stream, Task> read)
        {
            return blrepo.DB.ExecuteSingleQueryAsync(new QueryStreamedBlob<int>(this.ID, async sr => { await read(sr); return 0; }));
        }

        public TResult ReadStream<TResult>(Func<System.IO.Stream, TResult> read)
        {
            return blrepo.DB.ExecuteSingleQuery(new QueryStreamedBlob<TResult>(this.ID, read));
        }

        public void ReadStream(Action<System.IO.Stream> read)
        {
            blrepo.DB.ExecuteSingleQuery(new QueryStreamedBlob<int>(this.ID, sr => { read(sr); return 0; }));
        }
    }
}
