using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;

namespace IVO.Definition.Models
{
    /// <summary>
    /// A ref name.
    /// </summary>
    public sealed class RefName : PathObjectModel
    {
        internal RefName(IList<string> parts)
        {
            // parts must be already validated with validateCanonicalPath().
            this.Parts = new ReadOnlyCollection<string>(parts);
        }

        internal RefName(IEnumerable<string> parts, int initialCapacity = 4)
            : this(parts.ToList(initialCapacity))
        {
        }

        /// <summary>
        /// Gets the path components.
        /// </summary>
        public ReadOnlyCollection<string> Parts { get; private set; }

        public static explicit operator RefName(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new InvalidPathException("Ref name cannot be empty");

            string[] parts = SplitPath(path);

            validateCanonicalTreePath(parts);

            return new RefName(parts);
        }

        /// <summary>
        /// Renders the path as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Parts.Count == 0) return PathSeparatorString;

            return String.Join(PathSeparatorString, Parts);
        }
    }
}
