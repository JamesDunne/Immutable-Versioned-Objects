using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;

namespace IVO.Definition.Models
{
    public sealed class AbsoluteBlobPath : Path
    {
        public AbsoluteBlobPath(AbsoluteTreePath tree, string name)
        {
            this.Tree = tree;
            this.Name = name;
        }

        public AbsoluteTreePath Tree { get; private set; }
        public string Name { get; private set; }

        public static explicit operator AbsoluteBlobPath(string path)
        {
            if (path.Length == 0) throw new InvalidPathException("absolute path must begin with a '{0}'", PathSeparatorString);
            if (path[0] != PathSeparatorChar) throw new InvalidPathException("absolute path must begin with a '{0}'", PathSeparatorString);

            string[] parts = SplitPath(path);
            int treePartCount = parts.Length - 1;

            string[] treeParts = new string[treePartCount];
            Array.Copy(parts, treeParts, treePartCount);

            return new AbsoluteBlobPath((AbsoluteTreePath)treeParts, parts[treePartCount]);
        }

        public static explicit operator CanonicalBlobPath(AbsoluteBlobPath path)
        {
            return new CanonicalBlobPath(path.Tree.Canonicalize(), path.Name);
        }

        public CanonicalBlobPath Canonicalize()
        {
            return new CanonicalBlobPath(Tree.Canonicalize(), Name);
        }

        public override string ToString()
        {
            return String.Concat(Tree.ToString(), Name);
        }
    }
}
