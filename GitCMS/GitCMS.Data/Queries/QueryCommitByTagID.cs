using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Queries
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
SELECT {8} FROM {5} {6}{7} WHERE {6}.[tagid] = @tagid;
SELECT {1} FROM {2} {3}{4} WHERE {3}.[commitid] = @commitid;
SELECT [parent_commitid] FROM [dbo].[CommitParent] WHERE [commitid] = @commitid;",
                Tables.TablePKs_Commit.NameList("cm"),
                Tables.TablePKs_Commit.Concat(Tables.ColumnNames_Commit).NameList("cm", "cm_"),
                Tables.TableName_Commit,
                "cm",
                Tables.TableFromHint_Commit,
                Tables.TableName_Tag,
                "tg",
                Tables.TableFromHint_Tag,
                Tables.TablePKs_Tag.Concat(Tables.ColumnNames_Tag).NameList("tg", "tg_")
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@tagid", new SqlBinary((byte[])this._id));
            cmd.AddOutParameter("@commitid", System.Data.SqlDbType.Binary, 20);
            return cmd;
        }
        
        public Tuple<Tag, Commit> Retrieve(SqlDataReader dr, int expectedCapacity = 10)
        {
            return retrieve(dr);
        }

        internal static Tuple<Tag, Commit> retrieve(SqlDataReader dr)
        {
            // If no result, return null:
            if (!dr.Read()) return null;

            TagID tgid = (TagID)dr.GetSqlBinary(0).Value;
            Tag.Builder tgb = new Tag.Builder(
                pName:          dr.GetSqlString(1).Value,
                pCommitID:      (CommitID)dr.GetSqlBinary(2).Value,
                pTagger:        dr.GetSqlString(3).Value,
                pDateTagged:    dr.GetDateTimeOffset(4),
                pMessage:       dr.GetSqlString(5).Value
            );

            Tag tg = tgb;
            if (tg.ID != tgid) throw new InvalidOperationException();

            Commit cm = null;
            if (dr.NextResult() && dr.Read())
            {
                CommitID id = (CommitID)dr.GetSqlBinary(0).Value;

                Commit.Builder cmb = new Commit.Builder(
                    pParents: new System.Collections.Generic.List<CommitID>(10),
                    pTreeID: (TreeID)dr.GetSqlBinary(1).Value,
                    pCommitter: dr.GetSqlString(2).Value,
                    pDateCommitted: dr.GetDateTimeOffset(3),
                    pMessage: dr.GetSqlString(4).Value
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

                cm = cmb;
                if (cm.ID != id) throw new InvalidOperationException();
            }

            return new Tuple<Tag, Commit>(tg, cm);
        }
    }
}
