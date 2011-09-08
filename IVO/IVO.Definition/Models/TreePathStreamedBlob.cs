using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    public sealed class TreePathStreamBlob
    {
        public TreePathStreamBlob(TreeID rootTreeID, CanonicalBlobPath path, IStreamedBlob blob)
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
