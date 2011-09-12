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

        Task<Ref> GetRefByName(RefName name);

        Task<Ref> DeleteRefByName(RefName name);
    }
}
