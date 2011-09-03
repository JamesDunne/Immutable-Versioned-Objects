using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;
using System.Collections.ObjectModel;

namespace IVO.Definition.Models
{
    public sealed class CanonicalizedAbsolutePath
    {
        public const char PathSeparator = '/';
        public const string PathSeparatorString = "/";

        public static readonly char[] InvalidNameCharacters = new char[] { PathSeparator, ':', '$', '|', '&', '?', '%' };
        internal static readonly HashSet<char> invalidCharSet = new HashSet<char>(InvalidNameCharacters);

        private ReadOnlyCollection<string> _pathComponents;
        private string _asString;

        public CanonicalizedAbsolutePath(IEnumerable<string> pathComponents, int expectedCount = 4)
        {
            // Canonicalize the absolute path:
            List<string> parts = new List<string>(expectedCount);
            foreach (string part in pathComponents)
            {
                // Just ignore '.' paths:
                if (part == ".") continue;

                // Take a '..' and remove the last part of the path:
                if (part == "..")
                {
                    if (parts.Count == 0) throw new InvalidAbsolutePathException("");

                    // Remove the last part of the absolute path built so far:
                    parts.RemoveAt(parts.Count - 1);
                    continue;
                }

                // Make sure the part is a valid name:
                foreach (char ch in part)
                    if (invalidCharSet.Contains(ch))
                        throw new InvalidAbsolutePathException("One of the path components, '{0}', contains an invalid character '{1}'", part, ch);

                parts.Add(part);
            }
            
            this._pathComponents = parts.AsReadOnly();
            this._asString = PathSeparatorString + String.Join(PathSeparatorString, _pathComponents);
        }

        public CanonicalizedAbsolutePath(params string[] pathComponents)
            : this(pathComponents, pathComponents.Length)
        {
        }

        public ReadOnlyCollection<string> GetPathComponents() { return this._pathComponents; }

        public override string ToString()
        {
            return this._asString;
        }

        public static explicit operator string(CanonicalizedAbsolutePath path) { return path.ToString(); }
        public static implicit operator CanonicalizedAbsolutePath(string[] paths) { return new CanonicalizedAbsolutePath(paths); }

        public CanonicalizedAbsolutePath Concat(RelativePath path)
        {
            var otherParts = path.GetPathComponents();

            return new CanonicalizedAbsolutePath(this._pathComponents.Concat(otherParts), this._pathComponents.Count + otherParts.Count);
        }
    }
}
