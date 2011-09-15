using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Exceptions;
using System.Data;
using System.Threading.Tasks;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryCommit : IComplexDataQuery<Commit>
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

        public async Task<Commit> Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            // If no result, return null:
            if (!dr.Read()) return null;

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
            if (cm.ID != id) throw new CommitIDMismatchException(cm.ID, id);

            return cm;
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
