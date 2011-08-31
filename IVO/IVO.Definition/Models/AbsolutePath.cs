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

        private ReadOnlyCollection<string> _pathComponents;

        public AbsolutePath(params string[] pathComponents)
        {
            this._pathComponents = pathComponents.ToList(pathComponents.Length).AsReadOnly();
        }

        public AbsolutePath(IEnumerable<string> pathComponents)
        {
            this._pathComponents = pathComponents.ToList().AsReadOnly();
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
