using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Containers;

namespace IVO.Definition.Repositories
{
    public interface ITreeRepository
    {
        /// <summary>
        /// Persists an entire tree structure starting from the root TreeID (<paramref name="rootid"/>).
        /// </summary>
        /// <param name="rootid">The root TreeID to start persisting from.</param>
        /// <param name="trees">A container to find the Tree objects in.</param>
        /// <returns>The root Tree object.</returns>
        Task<Tree> PersistTree(TreeID rootid, ImmutableContainer<TreeID, Tree> trees);

        /// <summary>
        /// Gets a set of Tree objects by TreeIDs asynchronously.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<Tree> GetTree(TreeID id);
        
        /// <summary>
        /// Gets a set of Tree objects by TreeIDs asynchronously.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<Tree[]> GetTrees(params TreeID[] ids);

        /// <summary>
        /// Gets a TreeID from its path relative to a root TreeID.
        /// </summary>
        /// <param name="rootid"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<TreeIDPathMapping> GetTreeIDByPath(TreeTreePath path);

        /// <summary>
        /// Gets a TreeID from its path relative to a root TreeID.
        /// </summary>
        /// <param name="rootid"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<TreeIDPathMapping[]> GetTreeIDsByPaths(params TreeTreePath[] paths);

        /// <summary>
        /// Deletes an entire tree structure starting from the root TreeID (<paramref name="rootid"/>).
        /// </summary>
        /// <param name="rootid">The TreeID of the tree's root to delete.</param>
        /// <returns></returns>
        Task<TreeID> DeleteTreeRecursively(TreeID rootid);

        /// <summary>
        /// Retrieves an entire tree structure starting from the root TreeID (<paramref name="rootid"/>).
        /// </summary>
        /// <param name="rootid">The TreeID of the tree's root to retrieve.</param>
        /// <returns></returns>
        Task<Tuple<TreeID, ImmutableContainer<TreeID, Tree>>> GetTreeRecursively(TreeID rootid);

        /// <summary>
        /// Recursively retrieves all Tree objects from the absolute path from a root TreeID.
        /// </summary>
        /// <param name="rootid"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<Tuple<TreeID, ImmutableContainer<TreeID, Tree>>> GetTreeRecursivelyFromPath(TreeTreePath path);
    }
}
