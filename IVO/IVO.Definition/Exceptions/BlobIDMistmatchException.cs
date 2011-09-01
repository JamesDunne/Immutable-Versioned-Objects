using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Models;

namespace IVO.Definition.Exceptions
{
    public sealed class BlobIDMismatchException : Exception
    {
        public BlobIDMismatchException(BlobID constructedId, BlobID retrievedId)
            : base(String.Format("Constructed BlobID {0} does not match retrieved BlobID {1}", constructedId, retrievedId))
        { }
    }
}
