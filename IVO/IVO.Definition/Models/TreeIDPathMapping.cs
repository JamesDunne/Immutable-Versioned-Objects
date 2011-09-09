using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    public sealed class TreeIDPathMapping
    {
        public TreeIDPathMapping(TreeTreePath path, TreeID? id)
        {
            this.Path = path;
            this.TreeID = id;
        }

        public TreeTreePath Path { get; private set; }
        public TreeID? TreeID { get; private set; }
    }
}
