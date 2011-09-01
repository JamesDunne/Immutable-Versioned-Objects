using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Exceptions;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryCommitByTagID : IComplexDataQuery<Tuple<Tag, Commit>>
    {
        private TagID _id;

        public QueryCommitByTagID(TagID id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
@"SELECT @commitid = {0} FROM {2} {3}{4} JOIN {5} {6}{7} ON {3}.[commitid] = {6}.[commitid] WHERE {6}.[tagid] = @tagid;
SELECT {8},{1} FROM {2} {3}{4} JOIN {5} {6}{7} ON {3}.[commitid] = {6}.[commitid] WHERE {6}.[tagid] = @tagid;
SELECT [parent_commitid] FROM [dbo].[CommitParent] WHERE [commitid] = @commitid;",
                Tables.TablePKs_Commit.NameCommaList("cm"),
                Tables.TablePKs_Commit.Concat(Tables.ColumnNames_Commit).NameCommaListAs("cm", "cm_"),
                Tables.TableName_Commit,
                "cm",
                Tables.TableFromHint_Commit,
                Tables.TableName_Tag,
                "tg",
                Tables.TableFromHint_Tag,
                Tables.TablePKs_Tag.Concat(Tables.ColumnNames_Tag).NameCommaListAs("tg", "tg_")
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@tagid", new SqlBinary((byte[])this._id));
            cmd.AddOutParameter("@commitid", System.Data.SqlDbType.Binary, 20);
            return cmd;
        }

        public Tuple<Tag, Commit> Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return retrieve(cmd, dr);
        }

        internal static Tuple<Tag, Commit> retrieve(SqlCommand cmd, SqlDataReader dr)
        {
            // If no result, return null:
            if (!dr.Read()) return null;

            TagID tgid = (TagID)dr.GetSqlBinary(0).Value;
            Tag.Builder tgb = new Tag.Builder(
                pName: dr.GetSqlString(1).Value,
                pCommitID: (CommitID)dr.GetSqlBinary(2).Value,
                pTagger: dr.GetSqlString(3).Value,
                pDateTagged: dr.GetDateTimeOffset(4),
                pMessage: dr.GetSqlString(5).Value
            );

            Tag tg = tgb;
            if (tg.ID != tgid) throw new InvalidOperationException();

            const int offs = 6;
            CommitID id = (CommitID)dr.GetSqlBinary(0 + offs).Value;

            Commit.Builder cmb = new Commit.Builder(
                pParents: new System.Collections.Generic.List<CommitID>(2),
                pTreeID: (TreeID)dr.GetSqlBinary(1 + offs).Value,
                pCommitter: dr.GetSqlString(2 + offs).Value,
                pDateCommitted: dr.GetDateTimeOffset(3 + offs),
                pMessage: dr.GetSqlString(4 + offs).Value
            );

            // Read the parent commit ids from the second result:
            if (dr.NextResult())
            {
                while (dr.Read())
                {
                    cmb.Parents.Add((CommitID)dr.GetSqlBinary(0).Value);
                }
                cmb.Parents.Sort(new CommitID.Comparer());
            }

            Commit cm = cmb;
            if (cm.ID != id) throw new CommitIDMismatchException(cm.ID, id);

            return new Tuple<Tag, Commit>(tg, cm);
        }
    }
}
