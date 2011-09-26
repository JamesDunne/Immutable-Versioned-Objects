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
        Task<Errorable<Tag>> PersistTag(Tag tg);

        Task<Errorable<TagID>> DeleteTag(TagID id);

        Task<Errorable<TagID>> DeleteTagByName(TagName tagName);

        Task<Errorable<Tag>> GetTag(TagID id);

        Task<Errorable<Tag>> GetTagByName(TagName tagName);

        Task<FullQueryResponse<TagQuery, Tag>> SearchTags(TagQuery query);

        Task<OrderedFullQueryResponse<TagQuery, Tag, TagOrderBy>> SearchTags(TagQuery query, ReadOnlyCollection<OrderByApplication<TagOrderBy>> orderBy);

        Task<PagedQueryResponse<TagQuery, Tag, TagOrderBy>> SearchTags(TagQuery query, ReadOnlyCollection<OrderByApplication<TagOrderBy>> orderBy, PagingRequest paging);
    }
}
