using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Containers;

namespace IVO.Definition.Models
{
    public sealed class TreeTree
    {
        public TreeTree(TreeID rootID, ImmutableContainer<TreeID, TreeNode> trees)
        {
            this.RootID = rootID;
            this.Trees = trees;
        }

        public TreeID RootID { get; private set; }
        public ImmutableContainer<TreeID, TreeNode> Trees { get; private set; }
    }
}
