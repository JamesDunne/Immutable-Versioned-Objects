using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Errors
{
    public abstract class SemanticError : ErrorBase
    {
        protected SemanticError(string message) : base(message) { }
        protected SemanticError(string format, params object[] args) : base(format, args) { }
    }
}
