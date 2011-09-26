using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace IVO.Definition.Errors
{
    public abstract class ErrorBase
    {
        public abstract string Message { get; }
    }
}
