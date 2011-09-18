using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using System.Data;
using System.Threading.Tasks;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryCommitByTagName : IComplexDataQuery<Tuple<Tag, Commit>>
    {
        private TagName _tagName;

        public QueryCommitByTagName(TagName tagName)
        {
            this._tagName = tagName;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
@"SELECT @commitid = {0} FROM {2} {3}{4} JOIN {5} {6}{7} ON {3}.[commitid] = {6}.[commitid] WHERE {6}.[name] = @tagname;
SELECT {8},{1} FROM {2} {3}{4} JOIN {5} {6}{7} ON {3}.[commitid] = {6}.[commitid] WHERE {6}.[name] = @tagname;
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
            cmd.AddInParameter("@tagname", new SqlString(this._tagName.ToString()));
            cmd.AddOutParameter("@commitid", System.Data.SqlDbType.Binary, 20);
            return cmd;
        }

        public Task<Tuple<Tag, Commit>> RetrieveAsync(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return TaskEx.FromResult(QueryCommitByTagID.retrieve(cmd, dr));
        }

        public Tuple<Tag, Commit> Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return QueryCommitByTagID.retrieve(cmd, dr);
        }
        
        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
