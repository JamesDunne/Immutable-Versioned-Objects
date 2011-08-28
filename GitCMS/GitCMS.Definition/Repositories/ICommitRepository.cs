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
        Task<Commit> PersistCommit(Commit cm);

        Task<CommitID> DeleteCommit(CommitID id);

        Task<Commit> GetCommit(CommitID id);

        Task<Tuple<Tag, Commit>> GetCommitByTag(TagID id);

        Task<Tuple<Tag, Commit>> GetCommitByTagName(string tagName);

        Task<Tuple<Ref, Commit>> GetCommitByRef(string refName);
    }
}
