using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    /// <summary>
    /// A relative tree path is a path to a tree that is relative to an absolute tree path.
    /// </summary>
    public sealed class RelativeTreePath : PathObjectModel
    {
        private RelativeTreePath(IList<string> parts)
        {
            validateTreePath(parts);
            this.Parts = new ReadOnlyCollection<string>(parts);
        }

        private RelativeTreePath(IEnumerable<string> parts, int initialCapacity = 4)
            : this(parts.ToList(initialCapacity))
        {
        }

        /// <summary>
        /// Create a relative tree path from a string.
        /// </summary>
        /// <param name="path"></param>
        public RelativeTreePath(string path)
            : this(SplitPath(path))
        {
        }

        /// <summary>
        /// Gets the path components.
        /// </summary>
        public ReadOnlyCollection<string> Parts { get; private set; }

        public static RelativeTreePath operator +(RelativeTreePath root, RelativeTreePath rel)
        {
            return new RelativeTreePath(root.Parts.Concat(rel.Parts), root.Parts.Count + rel.Parts.Count);
        }

        public static RelativeBlobPath operator +(RelativeTreePath root, RelativeBlobPath rel)
        {
            return new RelativeBlobPath(root + rel.Tree, rel.Name);
        }

        public static explicit operator RelativeTreePath(string[] parts)
        {
            return new RelativeTreePath(parts);
        }

        public static explicit operator RelativeTreePath(string path)
        {
            return new RelativeTreePath(SplitPath(path));
        }

        /// <summary>
        /// Renders a path as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Concat(String.Join(PathSeparatorString, Parts), PathSeparatorString);
        }
    }
}
