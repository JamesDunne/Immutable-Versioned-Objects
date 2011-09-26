using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Containers;

namespace IVO.Definition.Models
{
    public sealed class CommitTree
    {
        public CommitTree(CommitID rootID, ImmutableContainer<CommitID, ICommit> commits)
        {
            this.RootID = rootID;
            this.Commits = commits;
        }

        public CommitID RootID { get; private set; }
        public ImmutableContainer<CommitID, ICommit> Commits { get; private set; }
    }
}
