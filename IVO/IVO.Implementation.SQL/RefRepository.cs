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
using IVO.Definition.Errors;

namespace IVO.Implementation.SQL
{
    public sealed class RefRepository : IRefRepository
    {
        private DataContext db;

        public RefRepository(DataContext db)
        {
            this.db = db;
        }

        public Task<Errorable<Ref>> PersistRef(Ref rf)
        {
            return db.ExecuteNonQueryAsync(new PersistRef(rf));
        }

        public Task<Errorable<Ref>> GetRefByName(RefName name)
        {
            return db.ExecuteSingleQueryAsync(new QueryRef(name));
        }

        public async Task<Errorable<Ref>> DeleteRefByName(RefName name)
        {
            // Grab the ref before we destroy it, just in case:
            var rf = await db.ExecuteSingleQueryAsync(new QueryRef(name));
            // Destroy it:
            await db.ExecuteNonQueryAsync(new DestroyRefByName(name));
            // Return the old ref data that was destroyed:
            return rf;
        }
    }
}
