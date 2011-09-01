using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Models;

namespace IVO.Definition.Exceptions
{
    public sealed class TreeIDMismatchException : Exception
    {
        public TreeIDMismatchException(TreeID constructedId, TreeID retrievedId)
            : base(String.Format("Constructed TreeID {0} does not match retrieved TreeID {1}", constructedId, retrievedId))
        { }
    }
}
