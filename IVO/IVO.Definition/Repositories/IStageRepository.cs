using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Errors;

namespace IVO.Definition.Repositories
{
    public interface IStageRepository
    {
        Task<Errorable<Stage>> PersistStage(Stage stg);

        Task<Errorable<Stage>> GetStageByName(StageName name);

        Task<Errorable<Stage>> DeleteStageByName(StageName name);
    }
}
