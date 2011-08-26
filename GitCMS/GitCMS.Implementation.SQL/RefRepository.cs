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
    public sealed class RefRepository : IRefRepository
    {
        private DataContext db;

        public RefRepository(DataContext db)
        {
            this.db = db;
        }

        public Task<Ref> PersistRef(Ref rf)
        {
            return db.AsynqNonQuery(new PersistRef(rf));
        }

        public Task<Ref> GetRef(string name)
        {
            return db.AsynqSingle(new QueryRef(name));
        }
    }
}
