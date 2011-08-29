using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asynq;
using IVO.Data;
using IVO.Data.Persists;
using IVO.Data.Queries;
using IVO.Definition;
using IVO.Definition.Models;
using IVO.Definition.Containers;
using IVO.Definition.Repositories;

namespace IVO.Implementation.SQL
{
    public sealed class CommitRepository : ICommitRepository 
    {
        private DataContext db;

        public CommitRepository(DataContext db)
        {
            this.db = db;
        }

        public Task<Commit> PersistCommit(Commit cm)
        {
            return db.ExecuteNonQueryAsync(new PersistCommit(cm));
        }

        public Task<CommitID> DeleteCommit(CommitID id)
        {
            return db.ExecuteNonQueryAsync(new DestroyCommit(id));
        }

        public Task<Commit> GetCommit(CommitID id)
        {
            return db.ExecuteSingleQueryAsync(new QueryCommit(id));
        }

        public Task<Tuple<Tag, Commit>> GetCommitByTag(TagID id)
        {
            return db.ExecuteSingleQueryAsync(new QueryCommitByTagID(id));
        }

        public Task<Tuple<Tag, Commit>> GetCommitByTagName(string tagName)
        {
            return db.ExecuteSingleQueryAsync(new QueryCommitByTagName(tagName));
        }

        public Task<Tuple<Ref, Commit>> GetCommitByRef(string refName)
        {
            return db.ExecuteSingleQueryAsync(new QueryCommitByRefName(refName));
        }

        public Task<Tuple<CommitID, CommitContainer>> GetCommitTree(CommitID id, int depth = 10)
        {
            throw new NotImplementedException();
        }
    }
}
