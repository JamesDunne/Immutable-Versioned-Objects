using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Errors;
using System.Collections.ObjectModel;

namespace IVO.Definition.Repositories
{
    public interface ITagRepository
    {
        Task<Either<Tag, PersistTagError>> PersistTag(Tag tg);

        Task<Either<TagID, DeleteTagError>> DeleteTag(TagID id);

        Task<Either<TagID, DeleteTagError>> DeleteTagByName(TagName tagName);

        Task<Either<Tag, GetTagError>> GetTag(TagID id);

        Task<Either<Tag, GetTagError>> GetTagByName(TagName tagName);

        Task<FullQueryResponse<TagQuery, Tag>> SearchTags(TagQuery query);

        Task<OrderedFullQueryResponse<TagQuery, Tag, TagOrderBy>> SearchTags(TagQuery query, ReadOnlyCollection<OrderByApplication<TagOrderBy>> orderBy);

        Task<PagedQueryResponse<TagQuery, Tag, TagOrderBy>> SearchTags(TagQuery query, ReadOnlyCollection<OrderByApplication<TagOrderBy>> orderBy, PagingRequest paging);
    }
}
