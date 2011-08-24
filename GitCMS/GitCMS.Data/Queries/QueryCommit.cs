using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Queries
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
@"SELECT {0} FROM {1}{2}{3} WHERE [commitid] = @id;
SELECT [parent_commitid] FROM [dbo].[CommitParents] WHERE [commitid] = @id;",
                Tables.TablePKs_Commit.Concat(Tables.ColumnNames_Commit).NameList(),
                Tables.TableName_Commit,
                "", // no alias
                Tables.TableFromHint_Commit
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@id", new SqlBinary((byte[])_id));
            return cmd;
        }

        public Commit Retrieve(SqlDataReader dr, int expectedCapacity = 10)
        {
            // If no result, return null:
            if (!dr.Read()) return null;

            CommitID id = (CommitID)dr.GetSqlBinary(0).Value;

            Commit.Builder b = new Commit.Builder(
                pParents:       new System.Collections.Generic.List<CommitID>(10),
                pTreeID:        (TreeID)dr.GetSqlBinary(1).Value,
                pCommitter:     dr.GetSqlString(2).Value,
                pAuthor:        dr.GetSqlString(3).Value,
                pDateCommitted: new DateTimeOffset(dr.GetSqlDateTime(4).Value),
                pMessage:       dr.GetSqlString(5).Value
            );

            // Read the parent commit ids from the second result:
            if (dr.NextResult())
            {
                while (dr.Read())
                {
                    b.Parents.Add((CommitID)dr.GetSqlBinary(0).Value);
                }
            }

            Commit cm = b;
            if (cm.ID != id) throw new InvalidOperationException();

            return cm;
        }
    }
}
