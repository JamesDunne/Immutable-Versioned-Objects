using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;
using System.Collections.ObjectModel;

namespace IVO.Definition.Models
{
    public sealed class RelativePath
    {
        private ReadOnlyCollection<string> _pathComponents;
        private string _asString;

        public RelativePath(IEnumerable<string> pathComponents, int expectedCount = 4)
        {
            // Canonicalize the absolute path:
            List<string> parts = new List<string>(expectedCount);
            foreach (string part in pathComponents)
            {
                // Make sure the part is a valid name:
                foreach (char ch in part)
                    if (CanonicalizedAbsolutePath.invalidCharSet.Contains(ch))
                        throw new InvalidAbsolutePathException("One of the path components, '{0}', contains an invalid character '{1}'", part, ch);

                parts.Add(part);
            }

            this._pathComponents = parts.AsReadOnly();
            this._asString = String.Join(CanonicalizedAbsolutePath.PathSeparatorString, _pathComponents);
        }

        public RelativePath(params string[] pathComponents)
            : this(pathComponents, pathComponents.Length)
        {
        }

        public ReadOnlyCollection<string> GetPathComponents() { return this._pathComponents; }

        public override string ToString()
        {
            return this._asString;
        }

        public static explicit operator string(RelativePath path) { return path.ToString(); }
        public static implicit operator RelativePath(string[] paths) { return new RelativePath(paths); }

        public RelativePath Concat(RelativePath path)
        {
            var otherParts = path.GetPathComponents();

            return new RelativePath(this._pathComponents.Concat(otherParts), this._pathComponents.Count + otherParts.Count);
        }
    }
}
