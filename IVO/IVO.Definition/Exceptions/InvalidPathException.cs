using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Exceptions
{
    public sealed class InvalidPathException : Exception
    {
        public InvalidPathException(string message) : base(message) { }
        public InvalidPathException(string format, params object[] args) : base(String.Format(format, args)) { }
    }
}
