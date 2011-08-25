using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitCMS.Definition.Models;

namespace GitCMS.Definition.Repositories
{
    public interface ICommitRepository
    {
        Task<int> PersistCommit(Commit cm);

        Task<Commit> GetCommit(CommitID id);

        Task<Commit> GetCommitByTag(TagID id);

        Task<Commit> GetCommitByTagName(string tagName);

        Task<Commit> GetCommitByRef(string refName);
    }
}
