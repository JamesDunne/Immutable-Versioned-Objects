using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Repositories;

namespace IVO.Implementation.FileSystem
{
    public sealed class RefRepository : IRefRepository
    {
        private FileSystem system;

        public RefRepository(FileSystem system)
        {
            this.system = system;
        }

        public Task<Ref> PersistRef(Ref rf)
        {
            throw new NotImplementedException();
        }

        public Task<Ref> GetRef(string name)
        {
            throw new NotImplementedException();
        }

        public Task<Ref> DestroyRefByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}
