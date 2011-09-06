using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;

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
            if (String.IsNullOrWhiteSpace(path)) throw new InvalidPathException("Path cannot be empty");
            if (path[0] != PathSeparatorChar) throw new InvalidPathException("Canonical blob path must begin with a '{0}'", PathSeparatorString);
            if (path[path.Length - 1] == PathSeparatorChar)
                throw new InvalidPathException("Canonical blob path cannot end in path separator character, '{0}'", PathSeparatorChar);

            string[] parts = SplitPath(path);
            validateCanonicalTreePath(parts);

            int treePartCount = parts.Length - 1;

            string[] treeParts = new string[treePartCount];
            Array.Copy(parts, treeParts, treePartCount);

            string blobName = parts[treePartCount];
            if (String.IsNullOrWhiteSpace(blobName))
                throw new InvalidPathException("Canonical blob path cannot end in path separator character, '{0}'", PathSeparatorChar);
            if (blobName == "." || blobName == "..")
                throw new InvalidPathException("Canonical blob path cannot end in '.' or '..' directory traversals");

            return new CanonicalBlobPath(new CanonicalTreePath(treeParts), blobName);
        }

        public override string ToString()
        {
            return String.Concat(Tree.ToString(), Name);
        }
    }
}
