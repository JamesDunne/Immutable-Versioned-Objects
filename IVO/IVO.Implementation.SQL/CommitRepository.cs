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
using IVO.Definition.Errors;

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

        public Task<Errorable<Commit>> PersistCommit(Commit cm)
        {
            return db.ExecuteNonQueryAsync(new PersistCommit(cm));
        }

        public Task<Errorable<CommitID>> DeleteCommit(CommitID id)
        {
            return db.ExecuteNonQueryAsync(new DestroyCommit(id));
        }

        public Task<Errorable<Commit>> GetCommit(CommitID id)
        {
            return db.ExecuteSingleQueryAsync(new QueryCommit(id));
        }

        public Task<Errorable<Tuple<Tag, Commit>>> GetCommitByTag(TagID id)
        {
            return db.ExecuteSingleQueryAsync(new QueryCommitByTagID(id));
        }

        public Task<Errorable<Tuple<Tag, Commit>>> GetCommitByTagName(TagName tagName)
        {
            return db.ExecuteSingleQueryAsync(new QueryCommitByTagName(tagName));
        }

        public Task<Errorable<Tuple<Ref, Commit>>> GetCommitByRefName(RefName refName)
        {
            return db.ExecuteSingleQueryAsync(new QueryCommitByRefName(refName));
        }

        public Task<Errorable<CommitTree>> GetCommitTree(CommitID id, int depth = 10)
        {
            return db.ExecuteListQueryAsync(new QueryCommitsRecursively(id, depth));
        }

        public async Task<Errorable<Tuple<Tag, CommitTree>>> GetCommitTreeByTagName(TagName tagName, int depth = 10)
        {
            // TODO: implement a single query to handle this
            var etg = await tgrepo.GetTagByName(tagName);
            if (etg.HasErrors) return etg.Errors;
            
            Tag tg = etg.Value;

            var ecmtr = await db.ExecuteListQueryAsync(new QueryCommitsRecursively(tg.CommitID, depth));
            if (ecmtr.HasErrors) return ecmtr.Errors;

            CommitTree cmtr = ecmtr.Value;
            return new Tuple<Tag, CommitTree>(tg, cmtr);
        }

        public async Task<Errorable<Tuple<Ref, CommitTree>>> GetCommitTreeByRefName(RefName refName, int depth = 10)
        {
            // TODO: implement a single query to handle this
            var erf = await rfrepo.GetRefByName(refName);
            if (erf.HasErrors) return erf.Errors;

            Ref rf = erf.Value;

            var ecmtr = await db.ExecuteListQueryAsync(new QueryCommitsRecursively(rf.CommitID, depth));
            if (ecmtr.HasErrors) return ecmtr.Errors;

            CommitTree cmtr = ecmtr.Value;
            return new Tuple<Ref, CommitTree>(rf, cmtr);
        }

        public async Task<Errorable<CommitID>> ResolvePartialID(CommitID.Partial id)
        {
            var resolvedIDs = await db.ExecuteListQueryAsync(new ResolvePartialCommitID(id));
            if (resolvedIDs.Length == 1) return resolvedIDs[0];
            if (resolvedIDs.Length == 0) return new CommitIDPartialNoResolutionError(id);
            return new CommitIDPartialAmbiguousResolutionError(id, resolvedIDs);
        }

        public Task<Errorable<CommitID>[]> ResolvePartialIDs(params CommitID.Partial[] ids)
        {
            return TaskEx.WhenAll(ids.SelectAsArray(id => ResolvePartialID(id)));
        }
    }
}
