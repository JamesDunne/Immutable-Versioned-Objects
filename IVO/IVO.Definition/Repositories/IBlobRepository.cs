using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Containers;

namespace IVO.Definition.Repositories
{
    public interface IBlobRepository
    {
        /// <summary>
        /// Persists multiple Blobs asynchronously.
        /// </summary>
        /// <param name="blobs"></param>
        /// <returns></returns>
        Task<BlobContainer> PersistBlobs(BlobContainer blobs);

        /// <summary>
        /// Deletes multiple Blobs by BlobIDs asynchronously.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<BlobID[]> DeleteBlobs(params BlobID[] ids);

        /// <summary>
        /// Retrieves multiple Blobs by BlobIDs asynchronously.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<Blob[]> GetBlobs(params BlobID[] ids);

        /// <summary>
        /// Gets a blob by its absolute path from a root TreeID.
        /// </summary>
        /// <param name="rootid"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<Blob> GetBlobByAbsolutePath(TreeID rootid, CanonicalizedAbsolutePath path);
    }
}
