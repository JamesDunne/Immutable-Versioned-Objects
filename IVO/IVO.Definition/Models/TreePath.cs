using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    /// <summary>
    /// Represents a canonical path to a blob relative to a root TreeID.
    /// </summary>
    public sealed class TreePath
    {
        public TreePath(TreeID rootTreeID, CanonicalBlobPath path)
        {
            this.RootTreeID = rootTreeID;
            this.Path = path;
        }

        public TreeID RootTreeID { get; private set; }
        public CanonicalBlobPath Path { get; private set; }
    }
}
