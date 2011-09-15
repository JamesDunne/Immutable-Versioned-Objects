using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;

namespace IVO.Definition.Repositories
{
    public interface ITreePathStreamedBlobRepository
    {
        /// <summary>
        /// Retrieves a single Blob by its canonicalized absolute path relative to its root TreeID asynchronously.
        /// </summary>
        /// <param name="rootid"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<TreePathStreamedBlob> GetBlobByTreePath(TreeBlobPath treePath);

        /// <summary>
        /// Retrieves multiple Blobs by their canonicalized absolute paths relative to their root TreeIDs asynchronously.
        /// </summary>
        /// <remarks>
        /// Input array length and order must be retained in the returned array.
        /// </remarks>
        /// <param name="rootid"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<TreePathStreamedBlob[]> GetBlobsByTreePaths(params TreeBlobPath[] treePaths);
    }
}
