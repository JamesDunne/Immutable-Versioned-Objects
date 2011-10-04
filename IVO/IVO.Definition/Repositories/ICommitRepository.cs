using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Containers;
using IVO.Definition.Errors;

namespace IVO.Definition.Repositories
{
    public interface ICommitRepository
    {
        Task<Errorable<CommitID>> ResolvePartialID(CommitID.Partial id);

        Task<Errorable<CommitID>[]> ResolvePartialIDs(params CommitID.Partial[] ids);

        Task<Errorable<Commit>> PersistCommit(Commit cm);

        Task<Errorable<CommitID>> DeleteCommit(CommitID id);

        Task<Errorable<Commit>> GetCommit(CommitID id);

        Task<Errorable<Tuple<Tag, Commit>>> GetCommitByTag(TagID id);

        Task<Errorable<Tuple<Tag, Commit>>> GetCommitByTagName(TagName tagName);

        Task<Errorable<Tuple<Ref, Commit>>> GetCommitByRefName(RefName refName);

        Task<Errorable<CommitTree>> GetCommitTree(CommitID id, int depth = 10);

        Task<Errorable<Tuple<Tag, CommitTree>>> GetCommitTreeByTagName(TagName tagName, int depth = 10);

        Task<Errorable<Tuple<Ref, CommitTree>>> GetCommitTreeByRefName(RefName refName, int depth = 10);
    }
}
