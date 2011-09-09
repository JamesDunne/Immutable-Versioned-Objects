using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Exceptions
{
    public sealed class ObjectParseException : Exception
    {
        public ObjectParseException(string message)
            : base(message)
        {
        }
    }
}
