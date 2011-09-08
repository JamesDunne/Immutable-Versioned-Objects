using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    /// <summary>
    /// Represents a streamed blob with its known absolute path relative to a root TreeID.
    /// </summary>
    public sealed class TreePathStreamedBlob
    {
        public TreePathStreamedBlob(TreeID rootTreeID, CanonicalBlobPath path, IStreamedBlob blob)
        {
            this.RootTreeID = rootTreeID;
            this.Path = path;
            this.StreamedBlob = blob;
        }

        public TreeID RootTreeID { get; private set; }
        public CanonicalBlobPath Path { get; private set; }
        public IStreamedBlob StreamedBlob { get; private set; }
    }
}
