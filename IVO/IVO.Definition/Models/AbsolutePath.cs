using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;
using System.Collections.ObjectModel;

namespace IVO.Definition.Models
{
    public sealed class AbsolutePath
    {
        public const char PathSeparator = '/';
        public const string PathSeparatorString = "/";

        public static readonly char[] InvalidNameCharacters = new char[] { PathSeparator, ':', '$', '|', '&', '?', '%' };
        internal static readonly HashSet<char> invalidCharSet = new HashSet<char>(InvalidNameCharacters);

        private ReadOnlyCollection<string> _pathComponents;

        public AbsolutePath(params string[] pathComponents)
        {
            this._pathComponents =
                pathComponents
                .Where(a => a.All(ch => (!invalidCharSet.Contains(ch)).Assert(b => b, b => new InvalidAbsolutePathException("One of the path components, '{0}', contains an invalid character '{1}'", a, ch))))
                .ToList(pathComponents.Length)
                .AsReadOnly();
        }

        public AbsolutePath(IEnumerable<string> pathComponents)
        {
            this._pathComponents =
                pathComponents
                .Where(a => a.All(ch => (!invalidCharSet.Contains(ch)).Assert(b => b, b => new InvalidAbsolutePathException("One of the path components, '{0}', contains an invalid character '{1}'", a, ch))))
                .ToList()
                .AsReadOnly();
        }

        public ReadOnlyCollection<string> GetPathComponents() { return this._pathComponents; }

        public override string ToString()
        {
            return PathSeparatorString + String.Join(PathSeparatorString, _pathComponents);
        }

        public static explicit operator string(AbsolutePath path) { return path.ToString(); }
        public static implicit operator AbsolutePath(string[] paths) { return new AbsolutePath(paths); }

        public AbsolutePath Concat(RelativePath path)
        {
            return new AbsolutePath(this._pathComponents.Concat(path.GetPathComponents()));
        }
    }
}
