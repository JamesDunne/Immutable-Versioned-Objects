using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using System.Collections.ObjectModel;

namespace IVO.Definition.Repositories
{
    public interface ITagRepository
    {
        Task<Tag> PersistTag(Tag tg);

        Task<TagID?> DeleteTag(TagID id);

        Task<TagID?> DeleteTagByName(TagName tagName);

        Task<Tag> GetTag(TagID id);

        Task<Tag> GetTagByName(TagName tagName);

        Task<ReadOnlyCollection<Tag>> SearchTags(TagQuery query);

        Task<OrderedResponse<Tag, TagOrderBy>> SearchTags(TagQuery query, ReadOnlyCollection<OrderByApplication<TagOrderBy>> orderBy);

        Task<PagedResponse<Tag, TagOrderBy>> SearchTags(TagQuery query, ReadOnlyCollection<OrderByApplication<TagOrderBy>> orderBy, PagingRequest paging);
    }
}
