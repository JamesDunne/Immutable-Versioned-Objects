using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;

namespace IVO.Definition.Repositories
{
    public interface IRefRepository
    {
        Task<Ref> PersistRef(Ref rf);

        Task<Ref> GetRef(string name);

        Task<Ref> DestroyRefByName(string name);
    }
}
