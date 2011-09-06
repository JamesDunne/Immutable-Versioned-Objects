using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    public sealed class RelativeTreePath : Path
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

        public ReadOnlyCollection<string> Parts { get; private set; }

        public static RelativeTreePath operator +(RelativeTreePath root, RelativeTreePath rel)
        {
            return new RelativeTreePath(root.Parts.Concat(rel.Parts), root.Parts.Count + rel.Parts.Count);
        }

        public static explicit operator RelativeTreePath(string[] parts)
        {
            return new RelativeTreePath(parts);
        }

        public static explicit operator RelativeTreePath(string path)
        {
            return new RelativeTreePath(SplitPath(path));
        }

        public override string ToString()
        {
            return String.Concat(String.Join(PathSeparatorString, Parts), PathSeparatorString);
        }
    }
}
