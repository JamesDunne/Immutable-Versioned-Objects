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
    public sealed class ResolvePartialTreeID : IComplexDataQuery<TreeID[]>
    {
        private TreeID.Partial _id;

        public ResolvePartialTreeID(TreeID.Partial id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
@"SELECT [{0}] FROM {1}{2}{3} WHERE LEFT(LEFT(RIGHT(master.dbo.fn_varbintohexstr([{0}]), 42), 40), LEN(@partialid)) = @partialid",
                Tables.TablePKs_Tree.Single(),
                Tables.TableName_Tree,
                "", // no alias
                Tables.TableFromHint_Tree
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@partialid", new SqlString(_id.ToString()));
            return cmd;
        }

        public Task<TreeID[]> RetrieveAsync(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return TaskEx.FromResult(retrieve(cmd, dr, expectedCapacity));
        }

        public TreeID[] Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return retrieve(cmd, dr, expectedCapacity);
        }

        private TreeID[] retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            List<TreeID> ids = new List<TreeID>(expectedCapacity);
            while (dr.Read())
            {
                ids.Add((TreeID)dr.GetSqlBinary(0).Value);
            }
            return ids.ToArray();
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
