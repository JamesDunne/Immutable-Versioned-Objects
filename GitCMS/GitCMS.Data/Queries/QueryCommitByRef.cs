using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Queries
{
    public sealed class QueryCommitByRefName : IComplexDataQuery<Tuple<Ref, Commit>>
    {
        private string _refName;

        public QueryCommitByRefName(string refName)
        {
            this._refName = refName;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
@"SELECT @commitid = {0} FROM {2} {3}{4} JOIN {5} {6}{7} ON {3}.[commitid] = {6}.[commitid] WHERE {6}.[name] = @refname;
SELECT {8} FROM {5} {6}{7} WHERE {6}.[name] = @refname;
SELECT {1} FROM {2} {3}{4} WHERE {3}.[commitid] = @commitid;
SELECT [parent_commitid] FROM [dbo].[CommitParent] WHERE [commitid] = @commitid;",
                Tables.TablePKs_Commit.NameList("cm"),
                Tables.TablePKs_Commit.Concat(Tables.ColumnNames_Commit).NameList("cm", "cm_"),
                Tables.TableName_Commit,
                "cm",
                Tables.TableFromHint_Commit,
                Tables.TableName_Ref,
                "rf",
                Tables.TableFromHint_Ref,
                Tables.TablePKs_Ref.Concat(Tables.ColumnNames_Ref).NameList("rf", "rf_")
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@refname", new SqlString(this._refName));
            cmd.AddOutParameter("@commitid", System.Data.SqlDbType.Binary, 20);
            return cmd;
        }

        public Tuple<Ref, Commit> Retrieve(SqlDataReader dr, int expectedCapacity = 10)
        {
            // If no result, return null:
            if (!dr.Read()) return null;

            Ref.Builder rfb = new Ref.Builder(
                pName:      dr.GetSqlString(0).Value,
                pCommitID:  (CommitID)dr.GetSqlBinary(1).Value
            );

            Ref rf = rfb;

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

            return new Tuple<Ref,Commit>(rf, cm);
        }
    }
}
