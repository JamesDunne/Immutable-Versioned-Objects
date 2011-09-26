using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IVO.Definition.Errors;

namespace IVO.Definition.Models
{
    /// <summary>
    /// An absolute tree path refers to a tree node with an absolute path that may contain directory traversals such as '.' and '..'.
    /// </summary>
    public sealed class AbsoluteTreePath : PathObjectModel
    {
        internal AbsoluteTreePath(IList<string> parts)
        {
            // parts must be validated with validatePath().
            this.Parts = new ReadOnlyCollection<string>(parts);
        }

        internal AbsoluteTreePath(IEnumerable<string> parts, int initialCapacity = 4)
        {
            // parts must be validated with validatePath().
            this.Parts = new ReadOnlyCollection<string>(parts.ToList(initialCapacity));
        }

        /// <summary>
        /// Gets the list of path components.
        /// </summary>
        public ReadOnlyCollection<string> Parts { get; private set; }

        public static AbsoluteTreePath operator +(AbsoluteTreePath root, RelativeTreePath rel)
        {
            return new AbsoluteTreePath(root.Parts.Concat(rel.Parts), root.Parts.Count + rel.Parts.Count);
        }

        public static AbsoluteBlobPath operator +(AbsoluteTreePath root, RelativeBlobPath rel)
        {
            return new AbsoluteBlobPath(new AbsoluteTreePath(root.Parts.Concat(rel.Tree.Parts), root.Parts.Count + rel.Tree.Parts.Count), rel.Name);
        }
        
        public static explicit operator AbsoluteTreePath(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new InvalidPathError("Path cannot be empty");

            // Remove trailing path separator char for parsing:
            if (path[path.Length - 1] == PathSeparatorChar) path = path.Substring(0, path.Length - 1);

            string[] parts = SplitPath(path);

            validateTreePath(parts);

            return new AbsoluteTreePath(parts);
        }

        /// <summary>
        /// Render the path as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Parts.Count == 0) return PathSeparatorString;

            return String.Concat(PathSeparatorString, String.Join(PathSeparatorString, Parts), PathSeparatorString);
        }

        /// <summary>
        /// Canonicalization normalizes an absolute path that may contain directory traversals like '.' or '..'
        /// to a path that cannot contain relative traversals and will always be a specific downward path from
        /// the root of the tree.
        /// </summary>
        /// <returns></returns>
        public CanonicalTreePath Canonicalize()
        {
            return CanonicalTreePath.Canonicalize(this);
        }
    }
}
