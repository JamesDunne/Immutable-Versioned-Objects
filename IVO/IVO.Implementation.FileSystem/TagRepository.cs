using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Repositories;

namespace IVO.Implementation.FileSystem
{
    public sealed class TagRepository : ITagRepository
    {
        private FileSystem system;

        public TagRepository(FileSystem system)
        {
            this.system = system;
        }

        public Task<Tag> PersistTag(Tag tg)
        {
            throw new NotImplementedException();
        }

        public Task<TagID> DeleteTag(TagID id)
        {
            throw new NotImplementedException();
        }

        public Task<TagID> DeleteTagByName(string tagName)
        {
            throw new NotImplementedException();
        }

        public Task<Tag> GetTag(TagID id)
        {
            throw new NotImplementedException();
        }

        public Task<Tag> GetTagByName(string tagName)
        {
            throw new NotImplementedException();
        }
    }
}
