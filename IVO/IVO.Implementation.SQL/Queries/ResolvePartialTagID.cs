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
    public sealed class ResolvePartialTagID : IComplexDataQuery<TagID[]>
    {
        private TagID.Partial _id;

        public ResolvePartialTagID(TagID.Partial id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
@"SELECT [{0}] FROM {1}{2}{3} WHERE LEFT(LEFT(RIGHT(master.dbo.fn_varbintohexstr([{0}]), 42), 40), LEN(@partialid)) = @partialid",
                Tables.TablePKs_Tag.Single(),
                Tables.TableName_Tag,
                "", // no alias
                Tables.TableFromHint_Tag
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@partialid", new SqlString(_id.ToString()));
            return cmd;
        }

        public Task<TagID[]> RetrieveAsync(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return Task.FromResult(retrieve(cmd, dr, expectedCapacity));
        }

        public TagID[] Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return retrieve(cmd, dr, expectedCapacity);
        }

        private TagID[] retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            List<TagID> ids = new List<TagID>(expectedCapacity);
            while (dr.Read())
            {
                ids.Add((TagID)dr.GetSqlBinary(0).Value);
            }
            return ids.ToArray();
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
