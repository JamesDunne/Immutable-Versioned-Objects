using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;

namespace IVO.Definition.Models
{
    public sealed class CanonicalTreePath : Path
    {
        private CanonicalTreePath(IList<string> parts)
        {
            validatePath(parts);
            this.Parts = new ReadOnlyCollection<string>(parts);
        }

        private CanonicalTreePath(IEnumerable<string> parts, int initialCapacity = 4)
            : this(parts.ToList(initialCapacity))
        {
        }

        public ReadOnlyCollection<string> Parts { get; private set; }

        public static AbsoluteTreePath operator +(CanonicalTreePath root, RelativeTreePath rel)
        {
            return new AbsoluteTreePath(root.Parts.Concat(rel.Parts), root.Parts.Count + rel.Parts.Count);
        }

        public static AbsoluteBlobPath operator +(CanonicalTreePath root, RelativeBlobPath rel)
        {
            return new AbsoluteBlobPath(new AbsoluteTreePath(root.Parts.Concat(rel.Tree.Parts), root.Parts.Count + rel.Tree.Parts.Count), rel.Name);
        }

        public override string ToString()
        {
            if (Parts.Count == 0) return PathSeparatorString;

            return String.Concat(PathSeparatorString, String.Join(PathSeparatorString, Parts), PathSeparatorString);
        }

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
                    if (parts.Count == 0) throw new InvalidPathException("traversed beyond the tree root with too many '..' path elements");

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
