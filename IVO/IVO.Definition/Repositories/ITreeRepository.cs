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
        Task<Tree> PersistTree(TreeID rootid, TreeContainer trees);

        /// <summary>
        /// Retrieves an entire tree structure starting from the root TreeID (<paramref name="rootid"/>).
        /// </summary>
        /// <param name="rootid">The TreeID of the tree's root to retrieve.</param>
        /// <returns></returns>
        Task<Tuple<TreeID, TreeContainer>> RetrieveTreeRecursively(TreeID rootid);

        /// <summary>
        /// Deletes an entire tree structure starting from the root TreeID (<paramref name="rootid"/>).
        /// </summary>
        /// <param name="rootid">The TreeID of the tree's root to delete.</param>
        /// <returns></returns>
        Task<TreeID> DeleteTreeRecursively(TreeID rootid);
    }
}
