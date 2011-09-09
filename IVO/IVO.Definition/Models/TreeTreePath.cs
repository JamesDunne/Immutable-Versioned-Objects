using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    /// <summary>
    /// Represents a canonical path to a tree relative to a root TreeID.
    /// </summary>
    public sealed class TreeTreePath
    {
        public TreeTreePath(TreeID rootTreeID, CanonicalTreePath path)
        {
            this.RootTreeID = rootTreeID;
            this.Path = path;
        }

        public TreeID RootTreeID { get; private set; }
        public CanonicalTreePath Path { get; private set; }
    }
}
