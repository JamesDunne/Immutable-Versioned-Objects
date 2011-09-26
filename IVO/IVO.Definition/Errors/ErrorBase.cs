using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace IVO.Definition.Errors
{
    public abstract class ErrorBase : Exception
    {
        protected ErrorBase(string message) : base(message)
        {
        }

        protected ErrorBase(string format, params object[] args)
            : base(String.Format(format, args))
        {
        }
    }
}
