using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asynq;
using IVO.Implementation.SQL;
using IVO.Implementation.SQL.Persists;
using IVO.Implementation.SQL.Queries;
using IVO.Definition;
using IVO.Definition.Models;
using IVO.Definition.Containers;
using IVO.Definition.Repositories;

namespace IVO.Implementation.SQL
{
    public sealed class CommitRepository : ICommitRepository 
    {
        private DataContext db;
        private TagRepository tgrepo;
        private RefRepository rfrepo;

        public CommitRepository(DataContext db)
        {
            this.db = db;
            this.tgrepo = new TagRepository(db);
            this.rfrepo = new RefRepository(db);
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

        public Task<Tuple<Tag, Commit>> GetCommitByTagName(TagName tagName)
        {
            return db.ExecuteSingleQueryAsync(new QueryCommitByTagName(tagName));
        }

        public Task<Tuple<Ref, Commit>> GetCommitByRefName(RefName refName)
        {
            return db.ExecuteSingleQueryAsync(new QueryCommitByRefName(refName));
        }

        public Task<Tuple<CommitID, ImmutableContainer<CommitID, ICommit>>> GetCommitTree(CommitID id, int depth = 10)
        {
            return db.ExecuteListQueryAsync(new QueryCommitsRecursively(id, depth));
        }

        public async Task<Tuple<Tag, CommitID, ImmutableContainer<CommitID, ICommit>>> GetCommitTreeByTagName(TagName tagName, int depth = 10)
        {
            // TODO: implement a single query to handle this
            var etg = await tgrepo.GetTagByName(tagName);
            if (etg.IsRight) return null;
            
            Tag tg = etg.Left;

            var cmtr = await db.ExecuteListQueryAsync(new QueryCommitsRecursively(tg.CommitID, depth));
            return new Tuple<Tag, CommitID, ImmutableContainer<CommitID, ICommit>>(tg, cmtr.Item1, cmtr.Item2);
        }

        public async Task<Tuple<Ref, CommitID, ImmutableContainer<CommitID, ICommit>>> GetCommitTreeByRefName(RefName refName, int depth = 10)
        {
            // TODO: implement a single query to handle this
            var rf = await rfrepo.GetRefByName(refName);
            if (rf == null) return null;

            var cmtr = await db.ExecuteListQueryAsync(new QueryCommitsRecursively(rf.CommitID, depth));
            return new Tuple<Ref, CommitID, ImmutableContainer<CommitID, ICommit>>(rf, cmtr.Item1, cmtr.Item2);
        }
    }
}
