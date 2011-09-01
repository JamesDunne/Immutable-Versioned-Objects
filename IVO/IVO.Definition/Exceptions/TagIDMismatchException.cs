using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Models;

namespace IVO.Definition.Exceptions
{
    public sealed class TagIDMismatchException : Exception
    {
        public TagIDMismatchException(TagID constructedId, TagID retrievedId)
            : base(String.Format("Constructed TagID {0} does not match retrieved TagID {1}", constructedId, retrievedId))
        { }
    }
}
