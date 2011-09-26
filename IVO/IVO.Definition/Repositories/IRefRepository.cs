using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Errors;

namespace IVO.Definition.Repositories
{
    public interface IRefRepository
    {
        Task<Errorable<Ref>> PersistRef(Ref rf);

        Task<Errorable<Ref>> GetRefByName(RefName name);

        Task<Errorable<Ref>> DeleteRefByName(RefName name);
    }
}
