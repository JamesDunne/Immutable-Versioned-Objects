using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Exceptions
{
    public sealed class InvalidAbsolutePathException : Exception
    {
        public InvalidAbsolutePathException(string message) : base(message) { }
        public InvalidAbsolutePathException(string format, params object[] args) : base(String.Format(format, args)) { }
    }
}
