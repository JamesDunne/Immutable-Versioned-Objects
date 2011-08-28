﻿using System;
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

        public Task<Commit> PersistCommit(Commit cm)
        {
            return db.AsynqNonQuery(new PersistCommit(cm));
        }

        public Task<CommitID> DeleteCommit(CommitID id)
        {
            throw new NotImplementedException();
        }

        public Task<Commit> GetCommit(CommitID id)
        {
            return db.AsynqSingle(new QueryCommit(id));
        }

        public Task<Tuple<Tag, Commit>> GetCommitByTag(TagID id)
        {
            return db.AsynqSingle(new QueryCommitByTagID(id));
        }

        public Task<Tuple<Tag, Commit>> GetCommitByTagName(string tagName)
        {
            return db.AsynqSingle(new QueryCommitByTagName(tagName));
        }

        public Task<Tuple<Ref, Commit>> GetCommitByRef(string refName)
        {
            return db.AsynqSingle(new QueryCommitByRefName(refName));
        }
    }
}
