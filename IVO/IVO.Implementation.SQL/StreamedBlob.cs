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
        private BlobRepository blrepo;

        internal StreamedBlob(BlobRepository blrepo, BlobID id)
        {
            this.ID = id;
            this.blrepo = blrepo;
        }

        public BlobID ID { get; private set; }

        public Task<TResult> ReadStream<TResult>(Func<System.IO.Stream, TResult> read)
        {
            return blrepo.DB.ExecuteSingleQueryAsync(new QueryStreamedBlob<TResult>(this.ID, read));
        }

        public Task ReadStream(Action<System.IO.Stream> read)
        {
            return blrepo.DB.ExecuteSingleQueryAsync(new QueryStreamedBlob<int>(this.ID, (sr) => { read(sr); return 0; }));
        }
    }
}
