using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Models;
using System.Threading.Tasks;
using IVO.Implementation.SQL.Queries;
using IVO.Definition.Errors;

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

        public Task<Errorable<TResult>> ReadStreamAsync<TResult>(Func<System.IO.Stream, Task<Errorable<TResult>>> read) where TResult : class
        {
            return blrepo.DB.ExecuteSingleQueryAsync(new QueryStreamedBlob<Errorable<TResult>>(this.ID, read));
        }

        public Task<Errorable> ReadStreamAsync(Func<System.IO.Stream, Task<Errorable>> read)
        {
            return blrepo.DB.ExecuteSingleQueryAsync(new QueryStreamedBlob<Errorable>(this.ID, async sr => { await read(sr); return Errorable.NoErrors; }));
        }

        public Errorable<TResult> ReadStream<TResult>(Func<System.IO.Stream, Errorable<TResult>> read) where TResult : class
        {
            return blrepo.DB.ExecuteSingleQuery(new QueryStreamedBlob<Errorable<TResult>>(this.ID, read));
        }

        public Errorable ReadStream(Func<System.IO.Stream, Errorable> read)
        {
            return blrepo.DB.ExecuteSingleQuery(new QueryStreamedBlob<Errorable>(this.ID, sr => { read(sr); return Errorable.NoErrors; }));
        }
    }
}
