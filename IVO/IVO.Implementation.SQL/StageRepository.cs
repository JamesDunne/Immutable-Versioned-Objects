using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Errors;
using IVO.Definition.Models;
using IVO.Definition.Repositories;

namespace IVO.Implementation.SQL
{
    public sealed class StageRepository : IStageRepository
    {
        public Task<Errorable<Stage>> PersistStage(Stage stg)
        {
            throw new NotImplementedException();
        }

        public Task<Errorable<Stage>> GetStageByName(StageName name)
        {
            throw new NotImplementedException();
        }

        public Task<Errorable<Stage>> DeleteStageByName(StageName name)
        {
            throw new NotImplementedException();
        }
    }
}
