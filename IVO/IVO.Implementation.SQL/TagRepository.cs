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

        public async Task<Either<Tag, PersistTagError>> PersistTag(Tag tg)
        {
            return await db.ExecuteNonQueryAsync(new PersistTag(tg));
        }

        public async Task<Either<Tag, GetTagError>> GetTag(TagID id)
        {
            Tag tg = await db.ExecuteSingleQueryAsync(new QueryTag(id));
            if (tg == null) return new GetTagError(GetTagError.ErrorType.TagIDFileDoesNotExist);
            return tg;
        }

        public async Task<Either<Tag, GetTagError>> GetTagByName(TagName tagName)
        {
            Tag tg = await db.ExecuteSingleQueryAsync(new QueryTag(tagName));
            if (tg == null) return new GetTagError(GetTagError.ErrorType.TagNameFileDoesNotExist);
            return tg;
        }

        public async Task<Either<TagID, DeleteTagError>> DeleteTag(TagID id)
        {
            TagID? delid = await db.ExecuteNonQueryAsync(new DestroyTag(id));
            if (!delid.HasValue) return new DeleteTagError(new GetTagError(GetTagError.ErrorType.TagIDFileDoesNotExist));
            return delid.Value;
        }

        public async Task<Either<TagID, DeleteTagError>> DeleteTagByName(TagName tagName)
        {
            TagID? delid = await db.ExecuteNonQueryAsync(new DestroyTagByName(tagName));
            if (!delid.HasValue) return new DeleteTagError(new GetTagError(GetTagError.ErrorType.TagNameFileDoesNotExist));
            return delid.Value;
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
    }
}
