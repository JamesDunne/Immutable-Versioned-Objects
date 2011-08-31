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

        public RelativePath(params string[] pathComponents)
        {
            this._pathComponents = pathComponents.ToList(pathComponents.Length).AsReadOnly();
        }

        public RelativePath(IEnumerable<string> pathComponents)
        {
            this._pathComponents = pathComponents.ToList().AsReadOnly();
        }

        public ReadOnlyCollection<string> GetPathComponents() { return this._pathComponents; }

        public override string ToString()
        {
            return String.Join(AbsolutePath.PathSeparatorString, _pathComponents);
        }

        public static explicit operator string(RelativePath path) { return path.ToString(); }
        public static implicit operator RelativePath(string[] paths) { return new RelativePath(paths); }
    }
}
