using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    /// <summary>
    /// Represents a streamed blob with its canonical path relative to a root TreeID.
    /// </summary>
    public sealed class TreePathStreamedBlob
    {
        public TreePathStreamedBlob(TreeBlobPath treePath, IStreamedBlob blob)
        {
            this.TreeBlobPath = treePath;
            this.StreamedBlob = blob;
        }

        public TreePathStreamedBlob(TreeID rootTreeID, CanonicalBlobPath path, IStreamedBlob blob)
            : this(new TreeBlobPath(rootTreeID, path), blob)
        {
        }

        public TreeBlobPath TreeBlobPath { get; private set; }
        public IStreamedBlob StreamedBlob { get; private set; }
    }
}
