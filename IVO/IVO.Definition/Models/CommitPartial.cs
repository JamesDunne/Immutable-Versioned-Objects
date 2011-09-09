using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    public sealed partial class CommitPartial : ICommit
    {
        public CommitID[] Parents
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsComplete
        {
            get { return false; }
        }
    }
}
