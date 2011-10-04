using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Errors
{
    public class ConsistencyError : ErrorBase
    {
        public ConsistencyError(string message) : base(message) { }
        public ConsistencyError(string format, params object[] args) : base(format, args) { }
    }
}
