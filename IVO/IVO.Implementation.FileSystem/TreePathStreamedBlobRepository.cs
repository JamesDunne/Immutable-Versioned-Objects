﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Repositories;

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

        public async Task<TreePathStreamedBlob[]> GetBlobsByTreePaths(params TreeBlobPath[] treePaths)
        {
#if false
            var blobs =
                from tp in treePaths
                let tr = await trrepo.GetTreeIDByPath(new TreeTreePath(tp.RootTreeID, tp.Path.Tree))
            // await operator cannot currently be used in query expressions.
#else
            // Estimated size:
            TreePathStreamedBlob[] blobs = new TreePathStreamedBlob[treePaths.Length];
            
            for (int i = 0; i < treePaths.Length; ++i)
            {
                var tp = treePaths[i];
                
                var trm = await trrepo.GetTreeIDByPath(new TreeTreePath(tp.RootTreeID, tp.Path.Tree));
                if (!trm.TreeID.HasValue)
                {
                    blobs[i] = null;
                    continue;
                }

                // Get the tree:
                var tr = await trrepo.GetTree(trm.TreeID.Value);
                
                // Get the blob out of this tree:
                // TODO: standardize name comparison semantics:
                var trbl = tr.Blobs.SingleOrDefault(x => x.Name == tp.Path.Name);
                
                // Finally return the streamed blob:
                // TODO: unknown length of blob; is it a problem?
                blobs[i] = new TreePathStreamedBlob(tp, new StreamedBlob(blrepo, trbl.BlobID));
            }

            return blobs;
#endif
        }
    }
}
