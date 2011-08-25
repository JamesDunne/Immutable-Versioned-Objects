using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asynq;
using GitCMS.Data;
using GitCMS.Data.Persists;
using GitCMS.Data.Queries;
using GitCMS.Definition;
using GitCMS.Definition.Models;
using GitCMS.Definition.Containers;
using GitCMS.Definition.Repositories;

namespace GitCMS.Implementation.SQL
{
    public sealed class CommitRepository : ICommitRepository 
    {
        private DataContext db;

        public CommitRepository(DataContext db)
        {
            this.db = db;
        }

        public Task<int> PersistCommit(Commit cm)
        {
            return db.AsynqNonQuery(new PersistCommit(cm));
        }

        public Task<Commit> GetCommit(CommitID id)
        {
            throw new NotImplementedException();
        }

        public Task<Commit> GetCommitByTag(TagID id)
        {
            throw new NotImplementedException();
        }

        public Task<Commit> GetCommitByTagName(string tagName)
        {
            throw new NotImplementedException();
        }

        public Task<Commit> GetCommitByRef(string refName)
        {
            throw new NotImplementedException();
        }

    }
}
