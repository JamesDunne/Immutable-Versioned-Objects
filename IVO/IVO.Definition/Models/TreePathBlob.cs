using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    public sealed class TreePathBlob
    {
        public TreePathBlob(TreeID rootTreeID, CanonicalBlobPath path, Blob blob)
        {
            this.RootTreeID = rootTreeID;
            this.Path = path;
            this.Blob = blob;
        }

        public TreeID RootTreeID { get; private set; }
        public CanonicalBlobPath Path { get; private set; }
        public Blob Blob { get; private set; }
    }
}
