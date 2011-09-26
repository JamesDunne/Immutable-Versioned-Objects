using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Errors
{
    public abstract class PersistenceError : ErrorBase
    {
        protected PersistenceError(string message) : base(message) { }
        protected PersistenceError(string format, params object[] args) : base(format, args) { }
    }
}
