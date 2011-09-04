using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;

namespace IVO.Definition.Models
{
    public sealed class AbsoluteTreePath : Path
    {
        internal AbsoluteTreePath(IList<string> parts)
        {
            validatePath(parts);
            this.Parts = new ReadOnlyCollection<string>(parts);
        }

        internal AbsoluteTreePath(IEnumerable<string> parts, int initialCapacity = 4)
            : this(parts.ToList(initialCapacity))
        {
        }

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
            return new AbsoluteTreePath(SplitPath(path));
        }

        public static explicit operator AbsoluteTreePath(string[] parts)
        {
            return new AbsoluteTreePath((string[])parts.Clone());
        }

        public override string ToString()
        {
            if (Parts.Count == 0) return PathSeparatorString;

            return String.Concat(PathSeparatorString, String.Join(PathSeparatorString, Parts), PathSeparatorString);
        }

        public CanonicalTreePath Canonicalize()
        {
            return CanonicalTreePath.Canonicalize(this);
        }
    }
}
