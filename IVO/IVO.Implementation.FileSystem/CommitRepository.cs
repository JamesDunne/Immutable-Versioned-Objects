using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Containers;
using IVO.Definition.Repositories;

namespace IVO.Implementation.FileSystem
{
    public sealed class CommitRepository : ICommitRepository
    {
        private FileSystem system;

        public CommitRepository(FileSystem system)
        {
            this.system = system;
        }

        public Task<Commit> PersistCommit(Commit cm)
        {
            throw new NotImplementedException();
        }

        public Task<CommitID> DeleteCommit(CommitID id)
        {
            throw new NotImplementedException();
        }

        public Task<Commit> GetCommit(CommitID id)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<Tag, Commit>> GetCommitByTag(TagID id)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<Tag, Commit>> GetCommitByTagName(string tagName)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<Ref, Commit>> GetCommitByRef(string refName)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<CommitID, ImmutableContainer<CommitID, ICommit>>> GetCommitTree(CommitID id, int depth = 10)
        {
            throw new NotImplementedException();
        }
    }
}
