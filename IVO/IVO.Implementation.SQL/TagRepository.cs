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

namespace IVO.Implementation.SQL
{
    public sealed class TagRepository : ITagRepository
    {
        private DataContext db;

        public TagRepository(DataContext db)
        {
            this.db = db;
        }

        public Task<Tag> PersistTag(Tag tg)
        {
            return db.ExecuteNonQueryAsync(new PersistTag(tg));
        }

        public Task<Tag> GetTag(TagID id)
        {
            return db.ExecuteSingleQueryAsync(new QueryTag(id));
        }

        public Task<Tag> GetTagByName(TagName tagName)
        {
            return db.ExecuteSingleQueryAsync(new QueryTag(tagName));
        }

        public Task<TagID?> DeleteTag(TagID id)
        {
            return db.ExecuteNonQueryAsync(new DestroyTag(id));
        }

        public Task<TagID?> DeleteTagByName(TagName tagName)
        {
            return db.ExecuteNonQueryAsync(new DestroyTagByName(tagName));
        }

        public Task<ReadOnlyCollection<Tag>> SearchTags(TagQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<OrderedResponse<Tag, TagOrderBy>> SearchTags(TagQuery query, ReadOnlyCollection<OrderByApplication<TagOrderBy>> orderBy)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResponse<Tag, TagOrderBy>> SearchTags(TagQuery query, ReadOnlyCollection<OrderByApplication<TagOrderBy>> orderBy, PagingRequest paging)
        {
            throw new NotImplementedException();
        }
    }
}
