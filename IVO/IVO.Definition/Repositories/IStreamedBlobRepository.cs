using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Containers;
using IVO.Definition.Errors;

namespace IVO.Definition.Repositories
{
    public interface IStreamedBlobRepository
    {
        /// <summary>
        /// Persists multiple Blobs asynchronously.
        /// </summary>
        /// <remarks>
        /// Input array length and order must be retained in the returned array.
        /// </remarks>
        /// <param name="blobs"></param>
        /// <returns></returns>
        Task<Errorable<IStreamedBlob>[]> PersistBlobs(params PersistingBlob[] blobs);

        Task<Errorable<IStreamedBlob>> PersistBlob(PersistingBlob blob);

        /// <summary>
        /// Deletes multiple Blobs by BlobIDs asynchronously.
        /// </summary>
        /// <remarks>
        /// Input array length and order must be retained in the returned array.
        /// </remarks>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<Errorable<BlobID>[]> DeleteBlobs(params BlobID[] ids);

        Task<Errorable<BlobID>> DeleteBlob(BlobID id);

        /// <summary>
        /// Retrieves multiple Blobs by BlobIDs asynchronously.
        /// </summary>
        /// <remarks>
        /// Input array length and order must be retained in the returned array.
        /// </remarks>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<Errorable<IStreamedBlob>[]> GetBlobs(params BlobID[] ids);

        Task<Errorable<IStreamedBlob>> GetBlob(BlobID id);

        /// <summary>
        /// Resolves a partial ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Errorable<BlobID>> ResolvePartialID(BlobID.Partial id);

        Task<Errorable<BlobID>[]> ResolvePartialIDs(params BlobID.Partial[] ids);
    }
}
