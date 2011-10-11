using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IVO.Definition.Errors;

namespace IVO.Definition.Models
{
    /// <summary>
    /// A canonicalized tree path is an absolute tree path that is guaranteed to contain no directory traversals and is in a normalized form.
    /// </summary>
    public sealed class CanonicalTreePath : PathObjectModel, IEquatable<CanonicalTreePath>
    {
        private string _asString;
        private CanonicalTreePath _parent;

        internal CanonicalTreePath(IList<string> parts)
        {
            // parts must be already validated with validateCanonicalPath().
            this.Parts = new ReadOnlyCollection<string>(parts);
            this._asString = (Parts.Count == 0) ? PathSeparatorString : String.Concat(PathSeparatorString, String.Join(PathSeparatorString, Parts), PathSeparatorString);
        }

        internal CanonicalTreePath(IEnumerable<string> parts, int initialCapacity = 4)
            : this(parts.ToList(initialCapacity))
        {
        }

        /// <summary>
        /// Gets the path components.
        /// </summary>
        public ReadOnlyCollection<string> Parts { get; private set; }

        public string Name { get { return Parts[Parts.Count - 1]; } }

        private CanonicalTreePath getPartialTree(int depth)
        {
            return new CanonicalTreePath(Parts.Take(depth), depth);
        }

        public CanonicalTreePath GetPartialTree(int depth)
        {
            if (depth < 0) throw new ArgumentNullException("depth");
            if (depth > Parts.Count) throw new ArgumentNullException("depth");

            return getPartialTree(depth);
        }

        public CanonicalTreePath GetParent()
        {
            return getPartialTree(Parts.Count - 1);
        }

        public static AbsoluteTreePath operator +(CanonicalTreePath root, RelativeTreePath rel)
        {
            return new AbsoluteTreePath(root.Parts.Concat(rel.Parts), root.Parts.Count + rel.Parts.Count);
        }

        public static AbsoluteBlobPath operator +(CanonicalTreePath root, RelativeBlobPath rel)
        {
            return new AbsoluteBlobPath(new AbsoluteTreePath(root.Parts.Concat(rel.Tree.Parts), root.Parts.Count + rel.Tree.Parts.Count), rel.Name);
        }

        public static explicit operator CanonicalTreePath(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new InvalidPathError("Path cannot be empty");

            // Remove trailing path separator char for parsing:
            if (path[path.Length - 1] == PathSeparatorChar) path = path.Substring(0, path.Length - 1);

            string[] parts = SplitPath(path);

            validateCanonicalTreePath(parts);

            return new CanonicalTreePath(parts);
        }

        public static implicit operator AbsoluteTreePath(CanonicalTreePath path)
        {
            return new AbsoluteTreePath(path.Parts);
        }

        public static bool operator ==(CanonicalTreePath a, CanonicalTreePath b)
        {
            bool aisnull = Object.ReferenceEquals(a, null);
            bool bisnull = Object.ReferenceEquals(b, null);

            if (aisnull && bisnull) return true;
            if (aisnull || bisnull) return false;

            return a._asString == b._asString;
        }

        public static bool operator !=(CanonicalTreePath a, CanonicalTreePath b)
        {
            bool aisnull = Object.ReferenceEquals(a, null);
            bool bisnull = Object.ReferenceEquals(b, null);

            if (aisnull && bisnull) return false;
            if (aisnull || bisnull) return true;

            return a._asString != b._asString;
        }

        public bool Equals(CanonicalTreePath other)
        {
            if (Object.ReferenceEquals(other, null)) return false;

            return _asString == other._asString;
        }

        /// <summary>
        /// Renders the path as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._asString;
        }

        /// <summary>
        /// Canonicalization normalizes an absolute path that may contain directory traversals like '.' or '..'
        /// to a path that cannot contain relative traversals and will always be a specific downward path from
        /// the root of the tree.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static CanonicalTreePath Canonicalize(AbsoluteTreePath path)
        {
            // Canonicalize the absolute path:
            List<string> parts = new List<string>(path.Parts.Count);

            foreach (string part in path.Parts)
            {
                // Just ignore '.' paths:
                if (part == ".") continue;

                // Take a '..' and remove the last part of the path:
                if (part == "..")
                {
                    if (parts.Count == 0) throw new InvalidPathError("traversed beyond the tree root with too many '..' path elements");

                    // Remove the last part of the absolute path built so far:
                    parts.RemoveAt(parts.Count - 1);
                    continue;
                }

                parts.Add(part);
            }

            return new CanonicalTreePath(parts);
        }
    }
}
