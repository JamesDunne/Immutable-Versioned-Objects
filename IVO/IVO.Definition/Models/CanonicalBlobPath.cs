using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    public sealed class CanonicalBlobPath : Path
    {
        public CanonicalBlobPath(CanonicalTreePath tree, string name)
        {
            this.Tree = tree;
            this.Name = name;
        }

        public CanonicalTreePath Tree { get; private set; }
        public string Name { get; private set; }

        public static explicit operator CanonicalBlobPath(string path)
        {
            return ((AbsoluteBlobPath)path).Canonicalize();
        }

        public override string ToString()
        {
            return String.Concat(Tree.ToString(), Name);
        }
    }
}
