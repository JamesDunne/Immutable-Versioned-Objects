﻿using System;
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

namespace IVO.Implementation.SQL
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
            return db.ExecuteNonQueryAsync(new PersistRef(rf));
        }

        public Task<Ref> GetRef(string name)
        {
            return db.ExecuteSingleQueryAsync(new QueryRef(name));
        }
    }
}