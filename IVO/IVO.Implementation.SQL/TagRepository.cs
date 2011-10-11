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
using System.Collections.ObjectModel;
using IVO.Definition.Errors;

namespace IVO.Implementation.SQL
{
    public sealed class TagRepository : ITagRepository
    {
        private DataContext db;

        public TagRepository(DataContext db)
        {
            this.db = db;
        }

        public async Task<Errorable<Tag>> PersistTag(Tag tg)
        {
            return await db.ExecuteNonQueryAsync(new PersistTag(tg));
        }

        public Task<Errorable<Tag>> GetTag(TagID id)
        {
            return db.ExecuteSingleQueryAsync(new QueryTag(id));
        }

        public Task<Errorable<Tag>> GetTagByName(TagName tagName)
        {
            return db.ExecuteSingleQueryAsync(new QueryTag(tagName));
        }

        public Task<Errorable<TagID>> DeleteTag(TagID id)
        {
            return db.ExecuteNonQueryAsync(new DestroyTag(id));
        }

        public Task<Errorable<TagID>> DeleteTagByName(TagName tagName)
        {
            return db.ExecuteNonQueryAsync(new DestroyTagByName(tagName));
        }

        public Task<FullQueryResponse<TagQuery, Tag>> SearchTags(TagQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<OrderedFullQueryResponse<TagQuery, Tag, TagOrderBy>> SearchTags(TagQuery query, ReadOnlyCollection<OrderByApplication<TagOrderBy>> orderBy)
        {
            throw new NotImplementedException();
        }

        public Task<PagedQueryResponse<TagQuery, Tag, TagOrderBy>> SearchTags(TagQuery query, ReadOnlyCollection<OrderByApplication<TagOrderBy>> orderBy, PagingRequest paging)
        {
            throw new NotImplementedException();
        }

        public async Task<Errorable<TagID>> ResolvePartialID(TagID.Partial id)
        {
            var resolvedIDs = await db.ExecuteListQueryAsync(new ResolvePartialTagID(id));
            if (resolvedIDs.Length == 1) return resolvedIDs[0];
            if (resolvedIDs.Length == 0) return new TagIDPartialNoResolutionError(id);
            return new TagIDPartialAmbiguousResolutionError(id, resolvedIDs);
        }

        public Task<Errorable<TagID>[]> ResolvePartialIDs(params TagID.Partial[] ids)
        {
            return Task.WhenAll(ids.SelectAsArray(id => ResolvePartialID(id)));
        }
    }
}
