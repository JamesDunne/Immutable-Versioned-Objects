using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using System.Collections.Generic;
using IVO.Definition.Containers;
using System.Diagnostics;

namespace IVO.Implementation.SQL.Queries
{
    public class QueryCommitsRecursively : IComplexDataQuery<Tuple<CommitID, CommitContainer>>
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
@";WITH Commits AS (
    SELECT      CAST(NULL AS binary(20)) AS commitid, cm.commitid AS parent_commitid, cm.treeid, cm.committer, cm.date_committed, cm.[message], 1 AS reclvl
    FROM        [dbo].[Commit] cm
    WHERE       cm.commitid = @commitid
    UNION ALL
    SELECT      cp.commitid, cp.parent_commitid, cm.treeid, cm.committer, cm.date_committed, cm.[message], 1 + parent.reclvl AS reclvl
    FROM        [dbo].[CommitParent] cp
    JOIN        [dbo].[Commit] cm ON cp.parent_commitid = cm.commitid
    JOIN        Commits parent ON parent.parent_commitid = cp.commitid
)
SELECT  cm.[commitid], cm.[parent_commitid], cm.treeid, cm.committer, cm.date_committed, cm.[message]
FROM    Commits cm
WHERE   cm.reclvl <= @depth";

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@commitid", new SqlBinary((byte[])this._id));
            cmd.AddInParameter("@depth", new SqlInt32(this._depth));
            return cmd;
        }

        public Tuple<CommitID, CommitContainer> Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCount)
        {
            Dictionary<CommitID, Commit.Builder> commits = new Dictionary<CommitID, Commit.Builder>(expectedCount);

            // Iterate through rows of the recursive query, assuming ordering of rows guarantees tree depth locality.
            while (dr.Read())
            {
                throw new NotImplementedException();
            }

            // Return the final result with immutable objects:
            return new Tuple<CommitID, CommitContainer>(
                this._id,
                new CommitContainer(
                    commits.Select(kv =>
                        // Verify that the retrieved ID is equivalent to the constructed ID:
                        ((Commit)kv.Value).With(cm => cm.Assert(kv.Key == cm.ID))
                    )
                )
            );
        }
    }
}
