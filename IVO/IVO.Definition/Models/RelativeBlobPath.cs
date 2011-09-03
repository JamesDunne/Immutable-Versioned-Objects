using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;

namespace IVO.Definition.Models
{
    public sealed class RelativeBlobPath : Path
    {
        public RelativeBlobPath(RelativeTreePath tree, string name)
        {
            this.Tree = tree;
            this.Name = name;
        }

        public RelativeTreePath Tree { get; private set; }
        public string Name { get; private set; }

        public static explicit operator RelativeBlobPath(string path)
        {
            if (path.Length == 0) throw new InvalidPathException("relative path cannot be empty");

            string[] parts = SplitPath(path);
            int treePartCount = parts.Length - 1;

            string[] treeParts = new string[treePartCount];
            Array.Copy(parts, treeParts, treePartCount);

            return new RelativeBlobPath((RelativeTreePath)treeParts, parts[treePartCount]);
        }

        public override string ToString()
        {
            return String.Concat(Tree.ToString(), Name);
        }
    }
}
