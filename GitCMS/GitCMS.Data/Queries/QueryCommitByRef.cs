using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Queries
{
    public sealed class QueryCommitByRef : IComplexDataQuery<Commit>
    {
        private string _refName;

        public QueryCommitByRef(string refName)
        {
            this._refName = refName;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
@"SELECT @commitid = {0} FROM {2}{3}{4}
JOIN {5}{6}{7} ON {3}.[commitid] = {6}.[commitid]
WHERE {6}.[name] = @refname;
SELECT {0}, {1} FROM {2}{3}{4} WHERE {3}.[commitid] = @commitid;
SELECT [parent_commitid] FROM [dbo].[CommitParent] WHERE [commitid] = @commitid;",
                Tables.TablePKs_Commit.NameList("cm"),
                Tables.ColumnNames_Commit.NameList("cm"),
                Tables.TableName_Commit,
                "cm", // no alias
                Tables.TableFromHint_Commit,
                Tables.TableName_Ref,
                "rf",
                Tables.TableFromHint_Ref
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@refname", new SqlString(this._refName));
            cmd.AddOutParameter("@commitid", System.Data.SqlDbType.Binary, 20);
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
                pDateCommitted: dr.GetDateTimeOffset(4),
                pMessage:       dr.GetSqlString(5).Value
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
            if (cm.ID != id) throw new InvalidOperationException();

            return cm;
        }
    }
}
