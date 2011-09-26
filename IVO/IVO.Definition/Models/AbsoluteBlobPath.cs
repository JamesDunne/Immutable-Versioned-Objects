using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Errors;

namespace IVO.Definition.Models
{
    /// <summary>
    /// An absolute blob path is a blob name hanging off of an absolute tree, which may contain directory traversals such as '.' and '..'.
    /// </summary>
    public sealed class AbsoluteBlobPath : PathObjectModel
    {
        /// <summary>
        /// Creates an absolute blob path from an absolute tree path and a blob name.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="name"></param>
        public AbsoluteBlobPath(AbsoluteTreePath tree, string name)
        {
            this.Tree = tree;
            this.Name = name;
        }

        /// <summary>
        /// Gets the absolute tree path that contains this blob.
        /// </summary>
        public AbsoluteTreePath Tree { get; private set; }
        /// <summary>
        /// Gets the blob name itself without any path components.
        /// </summary>
        public string Name { get; private set; }

        public static explicit operator AbsoluteBlobPath(string path)
        {
            if (path.Length == 0) throw new InvalidPathError("absolute blob path cannot be empty");
            if (path[0] != PathSeparatorChar) throw new InvalidPathError("absolute blob path must begin with a '{0}'", PathSeparatorString);

            string[] parts = SplitPath(path);
            validateTreePath(parts);

            int treePartCount = parts.Length - 1;

            string[] treeParts = new string[treePartCount];
            Array.Copy(parts, treeParts, treePartCount);

            string blobName = parts[treePartCount];
            if (String.IsNullOrWhiteSpace(blobName))
                throw new InvalidPathError("absolute blob path cannot end in a path separator character '{0}'", PathSeparatorChar);
            if (blobName == "." || blobName == "..")
                throw new InvalidPathError("absolute blob path cannot end in '.' or '..' directory traversals");

            return new AbsoluteBlobPath(new AbsoluteTreePath(treeParts), blobName);
        }

        public static explicit operator CanonicalBlobPath(AbsoluteBlobPath path)
        {
            return new CanonicalBlobPath(path.Tree.Canonicalize(), path.Name);
        }

        /// <summary>
        /// Render the path as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Concat(Tree.ToString(), Name);
        }

        /// <summary>
        /// Canonicalization normalizes an absolute path that may contain directory traversals like '.' or '..'
        /// to a path that cannot contain relative traversals and will always be a specific downward path from
        /// the root of the tree.
        /// </summary>
        /// <returns></returns>
        public CanonicalBlobPath Canonicalize()
        {
            return new CanonicalBlobPath(Tree.Canonicalize(), Name);
        }
    }
}
