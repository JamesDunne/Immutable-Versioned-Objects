using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Errors
{
    public sealed class InvalidPathError : InputError
    {
        public InvalidPathError(string message) : base(message) { }
        public InvalidPathError(string format, params object[] args) : base(format, args) { }
    }
}
