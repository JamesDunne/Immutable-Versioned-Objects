using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using System.Collections.Generic;
using IVO.Definition.Containers;
using System.Diagnostics;
using IVO.Definition.Exceptions;

namespace IVO.Implementation.SQL.Queries
{
    public class QueryCommitsRecursively : IComplexDataQuery<Tuple<CommitID, ICommitContainer>>
    {
        private CommitID _id;
        private int _depth;

        public QueryCommitsRecursively(CommitID id, int depth = 10)
        {
            this._id = id;
            this._depth = depth;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText =
@";WITH cmtree AS (
    SELECT      CAST(NULL AS binary(20)) AS commitid, cm.commitid AS parent_commitid, cm.treeid, cm.committer, cm.date_committed, cm.[message], 1 AS depth
    FROM        [dbo].[Commit] cm WITH (NOLOCK)
    WHERE       cm.commitid = @commitid
    UNION ALL
    SELECT      cp.commitid, cp.parent_commitid, cm.treeid, cm.committer, cm.date_committed, cm.[message], 1 + parent.depth AS depth
    FROM        [dbo].[CommitParent] cp WITH (NOLOCK)
    JOIN        [dbo].[Commit] cm WITH (NOLOCK) ON cp.parent_commitid = cm.commitid
    JOIN        cmtree parent ON parent.parent_commitid = cp.commitid
)
SELECT  cm.[commitid], cm.[parent_commitid], cm.treeid, cm.committer, cm.date_committed, cm.[message], cm.depth
FROM    cmtree cm
WHERE   cm.depth <= @depth";

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@commitid", new SqlBinary((byte[])this._id));
            cmd.AddInParameter("@depth", new SqlInt32(this._depth));
            return cmd;
        }

        public Tuple<CommitID, ICommitContainer> Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCount)
        {
            Dictionary<CommitID, Commit.Builder> commits = new Dictionary<CommitID, Commit.Builder>(expectedCount);
            CommitPartial.Builder cmPartial = null;

            // Iterate through rows of the recursive query, assuming ordering of rows guarantees tree depth locality.
            while (dr.Read())
            {
                SqlBinary b0 = dr.GetSqlBinary(0);
                SqlBinary b1 = dr.GetSqlBinary(1);

                CommitID? cmid = b0.IsNull ? (CommitID?)null : (CommitID?)b0.Value;
                CommitID? parent_cmid = b1.IsNull ? (CommitID?)null : (CommitID?)b1.Value;

                int depth = dr.GetSqlInt32(6).Value;
                if (depth == this._depth)
                {
                    // This should be the last record and it is partial because its parent CommitIDs are unknown:
                    cmPartial = new CommitPartial.Builder(
                        pID:            parent_cmid.Value,
                        pTreeID:        (TreeID)dr.GetSqlBinary(2).Value,
                        pCommitter:     dr.GetSqlString(3).Value,
                        pDateCommitted: dr.GetDateTimeOffset(4),
                        pMessage:       dr.GetSqlString(5).Value
                    );

                    if (cmid.HasValue)
                        commits[cmid.Value].Parents.Add(parent_cmid.Value);

                    Debug.Assert(!dr.Read());
                    break;
                }

                // Create a commit builder:
                Commit.Builder cmb = new Commit.Builder(
                    pParents:       new List<CommitID>(2),
                    pTreeID:        (TreeID)dr.GetSqlBinary(2).Value,
                    pCommitter:     dr.GetSqlString(3).Value,
                    pDateCommitted: dr.GetDateTimeOffset(4),
                    pMessage:       dr.GetSqlString(5).Value
                );
                commits.Add(parent_cmid.Value, cmb);

                if (cmid.HasValue)
                    commits[cmid.Value].Parents.Add(parent_cmid.Value);
            }

            // Return the final result with immutable objects:
            return new Tuple<CommitID, ICommitContainer>(
                this._id,
                new ICommitContainer(
                    commits.Select(kv =>
                        // Verify that the retrieved ID is equivalent to the constructed ID:
                        (ICommit) ((Commit)kv.Value).Assert(cm => kv.Key == cm.ID, cm => new CommitIDMismatchException("Constructed CommitID {0} does not match retrieved CommitID {1}", cm.ID, kv.Key))
                    ).Concat(
                        // Add the partial commit if there is one:
                        (cmPartial == null)
                          ? new ICommit[0]
                          : new ICommit[1] { (CommitPartial)cmPartial }
                    )
                )
            );
        }
    }
}
