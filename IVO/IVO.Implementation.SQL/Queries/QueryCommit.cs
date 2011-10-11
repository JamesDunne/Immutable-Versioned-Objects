using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Errors;
using System.Data;
using System.Threading.Tasks;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryCommit : IComplexDataQuery<Errorable<Commit>>
    {
        private CommitID _id;

        public QueryCommit(CommitID id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
@"SELECT {0} FROM {1}{2}{3} WHERE [commitid] = @commitid;
SELECT [parent_commitid] FROM [dbo].[CommitParent] WHERE [commitid] = @commitid;",
                Tables.TablePKs_Commit.Concat(Tables.ColumnNames_Commit).NameCommaList(),
                Tables.TableName_Commit,
                "", // no alias
                Tables.TableFromHint_Commit
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@commitid", new SqlBinary((byte[])_id));
            return cmd;
        }

        public Task<Errorable<Commit>> RetrieveAsync(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return Task.FromResult(retrieve(cmd, dr));
        }

        public Errorable<Commit> Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return retrieve(cmd, dr);
        }

        private Errorable<Commit> retrieve(SqlCommand cmd, SqlDataReader dr)
        {
            // If no result, return null:
            if (!dr.Read()) return new CommitIDRecordDoesNotExistError(this._id);

            CommitID id = (CommitID)dr.GetSqlBinary(0).Value;

            Commit.Builder b = new Commit.Builder(
                pParents:       new System.Collections.Generic.List<CommitID>(10),
                pTreeID:        (TreeID)dr.GetSqlBinary(1).Value,
                pCommitter:     dr.GetSqlString(2).Value,
                pDateCommitted: dr.GetDateTimeOffset(3),
                pMessage:       dr.GetSqlString(4).Value
            );

            // Read the parent commit ids from the second result:
            if (dr.NextResult())
            {
                while (dr.Read())
                {
                    b.Parents.Add((CommitID)dr.GetSqlBinary(0).Value);
                }
                b.Parents.Sort(new CommitID.Comparer());
            }

            Commit cm = b;
            if (cm.ID != id) throw new ComputedCommitIDMismatchError(cm.ID, id);

            return cm;
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
