using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Containers;

namespace IVO.Definition.Models
{
    public sealed class TreeTree
    {
        public TreeTree(TreeID rootID, ImmutableContainer<TreeID, Tree> trees)
        {
            this.RootID = rootID;
            this.Trees = trees;
        }

        public TreeID RootID { get; private set; }
        public ImmutableContainer<TreeID, Tree> Trees { get; private set; }
    }
}
