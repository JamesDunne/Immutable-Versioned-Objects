using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Errors
{
    public abstract class InputError : ErrorBase
    {
        protected InputError(string message) : base(message) { }
        protected InputError(string format, params object[] args) : base(format, args) { }
    }
}
