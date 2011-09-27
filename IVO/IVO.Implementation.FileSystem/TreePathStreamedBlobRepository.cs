using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using IVO.Definition.Errors;

namespace IVO.Implementation.FileSystem
{
    public sealed class TreePathStreamedBlobRepository : ITreePathStreamedBlobRepository
    {
        private FileSystem system;
        private StreamedBlobRepository blrepo;
        private TreeRepository trrepo;

        public TreePathStreamedBlobRepository(FileSystem system, TreeRepository trrepo = null, StreamedBlobRepository blrepo = null)
        {
            this.system = system;
            this.trrepo = trrepo ?? new TreeRepository(system);
            this.blrepo = blrepo ?? new StreamedBlobRepository(system);
        }

        public async Task<Errorable<TreePathStreamedBlob>[]> GetBlobsByTreePaths(params TreeBlobPath[] treePaths)
        {
#if false
            var blobs =
                from tp in treePaths
                let tr = await trrepo.GetTreeIDByPath(new TreeTreePath(tp.RootTreeID, tp.Path.Tree))
            // await operator cannot currently be used in query expressions.
#else
            // Estimated size:
            var blobs = new Errorable<TreePathStreamedBlob>[treePaths.Length];
            
            for (int i = 0; i < treePaths.Length; ++i)
            {
                var tp = treePaths[i];

                var etrm = await trrepo.GetTreeIDByPath(new TreeTreePath(tp.RootTreeID, tp.Path.Tree)).ConfigureAwait(continueOnCapturedContext: false);
                if (etrm.HasErrors) blobs[i] = etrm.Errors;

                var trm = etrm.Value;
                if (!trm.TreeID.HasValue)
                {
                    blobs[i] = null;
                    continue;
                }

                // Get the tree:
                var etr = await trrepo.GetTree(trm.TreeID.Value).ConfigureAwait(continueOnCapturedContext: false);
                if (etr.HasErrors) blobs[i] = etr.Errors;

                TreeNode tr = etr.Value;

                // Get the blob out of this tree:
                // TODO: standardize name comparison semantics:
                var trbl = tr.Blobs.SingleOrDefault(x => x.Name == tp.Path.Name);
                if (trbl == null)
                {
                    blobs[i] = null;
                    continue;
                }
                
                // Finally return the streamed blob:
                // TODO: unknown length of blob; is it a problem?
                blobs[i] = new TreePathStreamedBlob(tp, new StreamedBlob(blrepo, trbl.BlobID));
            }

            return blobs;
#endif
        }

        public async Task<Errorable<TreePathStreamedBlob>> GetBlobByTreePath(TreeBlobPath treePath)
        {
            var etrm = await trrepo.GetTreeIDByPath(new TreeTreePath(treePath.RootTreeID, treePath.Path.Tree)).ConfigureAwait(continueOnCapturedContext: false);
            if (etrm.HasErrors) return etrm.Errors;
            
            TreeIDPathMapping trm = etrm.Value;
            if (!trm.TreeID.HasValue) return new BlobNotFoundByPathError();

            // Get the tree:
            var etr = await trrepo.GetTree(trm.TreeID.Value).ConfigureAwait(continueOnCapturedContext: false);
            if (etr.HasErrors) return etr.Errors;

            TreeNode tr = etr.Value;

            // Get the blob out of this tree:
            // TODO: standardize name comparison semantics:
            var trbl = tr.Blobs.SingleOrDefault(x => x.Name == treePath.Path.Name);
            if (trbl == null) return new BlobNotFoundByPathError();

            // System inconsistency!
            if (!system.getPathByID(trbl.BlobID).Exists) return new BlobNotFoundByPathError();

            return new TreePathStreamedBlob(treePath, new StreamedBlob(blrepo, trbl.BlobID));
        }
    }
}
