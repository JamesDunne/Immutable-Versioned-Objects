using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Exceptions
{
    public sealed class TreeIDMismatchException : Exception
    {
        public TreeIDMismatchException(string message) : base(message) { }
        public TreeIDMismatchException(string format, params object[] args) : base(String.Format(format, args)) { }
    }
}
