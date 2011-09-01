using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Models;

namespace IVO.Definition.Exceptions
{
    public sealed class CommitIDMismatchException : Exception
    {
        public CommitIDMismatchException(CommitID constructedId, CommitID retrievedId)
            : base(String.Format("Constructed CommitID {0} does not match retrieved CommitID {1}", constructedId, retrievedId))
        { }
    }
}
