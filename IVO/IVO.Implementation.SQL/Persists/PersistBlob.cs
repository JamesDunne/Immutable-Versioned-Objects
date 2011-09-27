using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Errors;

namespace IVO.Implementation.SQL.Persists
{
    public sealed class PersistBlob : IDataOperation<Errorable<IStreamedBlob>>
    {
        private PersistingBlob _bl;
        private StreamedBlobRepository _blrepo;
        private long _length;

        public PersistBlob(StreamedBlobRepository blrepo, PersistingBlob bl)
        {
            this._bl = bl;
            this._blrepo = blrepo;
            this._length = -1;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            // TODO: stream blob contents into database with UPDATE blob SET contents.WRITE(@chunk, NULL, NULL)
            // to append to contents, initially supply a temporary blobid (use an extra byte, binary(21) and set
            // the last byte to 0xFF to indicate temporary, 0x00 indicates normal).
            // Create a Write-only Stream class that uploads contents to the blob table using above.
            // Use the Write-only Stream wrapped in SHA1StreamWriter to calculate SHA1 during writing.
            // Get the final BlobID from the SHA1StreamWriter and update the blobid of the temporary record.
            return null;
        }

        public Errorable<IStreamedBlob> Return(SqlCommand cmd, int rowsAffected)
        {
            return new StreamedBlob(_blrepo, new BlobID(), _length);
        }
    }
}
