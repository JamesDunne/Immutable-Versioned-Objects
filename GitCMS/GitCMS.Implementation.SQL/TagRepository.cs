using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asynq;
using GitCMS.Data;
using GitCMS.Data.Persists;
using GitCMS.Data.Queries;
using GitCMS.Definition;
using GitCMS.Definition.Models;
using GitCMS.Definition.Containers;
using GitCMS.Definition.Repositories;

namespace GitCMS.Implementation.SQL
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
            return db.AsynqNonQuery(new PersistTag(tg));
        }

        public Task<Tag> GetTag(TagID id)
        {
            return db.AsynqSingle(new QueryTag(id));
        }

        public Task<Tag> GetTagByName(string tagName)
        {
            return db.AsynqSingle(new QueryTag(tagName));
        }

        public Task<TagID> DeleteTag(TagID id)
        {
            return db.AsynqNonQuery(new DestroyTag(id));
        }

        public Task<TagID> DeleteTagByName(string tagName)
        {
            return db.AsynqNonQuery(new DestroyTagByName(tagName));
        }
    }
}
