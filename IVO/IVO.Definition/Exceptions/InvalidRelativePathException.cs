using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Exceptions
{
    public sealed class InvalidRelativePathException : Exception
    {
        public InvalidRelativePathException(string message) : base(message) { }
        public InvalidRelativePathException(string format, params object[] args) : base(String.Format(format, args)) { }
    }
}
