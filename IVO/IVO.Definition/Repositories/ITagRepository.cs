using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;

namespace IVO.Definition.Repositories
{
    public interface ITagRepository
    {
        Task<Tag> PersistTag(Tag tg);

        Task<TagID?> DeleteTag(TagID id);

        Task<TagID?> DeleteTagByName(string tagName);

        Task<Tag> GetTag(TagID id);

        Task<Tag> GetTagByName(string tagName);
    }
}
