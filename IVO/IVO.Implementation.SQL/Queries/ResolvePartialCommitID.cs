using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Errors;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class ResolvePartialCommitID : IComplexDataQuery<CommitID[]>
    {
        private CommitID.Partial _id;

        public ResolvePartialCommitID(CommitID.Partial id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
@"SELECT [{0}] FROM {1}{2}{3} WHERE LEFT(LEFT(RIGHT(master.dbo.fn_varbintohexstr([{0}]), 42), 40), LEN(@partialid)) = @partialid",
                Tables.TablePKs_Commit.Single(),
                Tables.TableName_Commit,
                "", // no alias
                Tables.TableFromHint_Commit
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@partialid", new SqlString(_id.ToString()));
            return cmd;
        }

        public Task<CommitID[]> RetrieveAsync(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return Task.FromResult(retrieve(cmd, dr, expectedCapacity));
        }

        public CommitID[] Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return retrieve(cmd, dr, expectedCapacity);
        }

        private CommitID[] retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            List<CommitID> ids = new List<CommitID>(expectedCapacity);
            while (dr.Read())
            {
                ids.Add((CommitID)dr.GetSqlBinary(0).Value);
            }
            return ids.ToArray();
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
