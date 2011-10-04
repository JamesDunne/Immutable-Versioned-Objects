using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Errors
{
    public class InputError : ErrorBase
    {
        public InputError(string message) : base(message) { }
        public InputError(string format, params object[] args) : base(format, args) { }
    }
}
