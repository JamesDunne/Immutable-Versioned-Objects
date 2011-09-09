using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    public interface ICommit
    {
        CommitID ID { get; }
        CommitID[] Parents { get; }
        TreeID TreeID { get; }
        string Committer { get; }
        DateTimeOffset DateCommitted { get; }
        string Message { get; }

        bool IsComplete { get; }
    }
}
