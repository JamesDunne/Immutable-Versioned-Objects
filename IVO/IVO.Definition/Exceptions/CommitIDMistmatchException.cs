using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Exceptions
{
    public sealed class CommitIDMismatchException : Exception
    {
        public CommitIDMismatchException(string message) : base(message) { }
        public CommitIDMismatchException(string format, params object[] args) : base(String.Format(format, args)) { }
    }
}
