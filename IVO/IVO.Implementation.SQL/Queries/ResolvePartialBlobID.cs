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
    public sealed class ResolvePartialBlobID : IComplexDataQuery<BlobID[]>
    {
        private BlobID.Partial _id;

        public ResolvePartialBlobID(BlobID.Partial id)
        {
            this._id = id;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
@"SELECT [{0}] FROM {1}{2}{3} WHERE LEFT(LEFT(RIGHT(master.dbo.fn_varbintohexstr([{0}]), 42), 40), LEN(@partialid)) = @partialid",
                Tables.TablePKs_Blob.Single(),
                Tables.TableName_Blob,
                "", // no alias
                Tables.TableFromHint_Blob
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@partialid", new SqlString(_id.ToString()));
            return cmd;
        }

        public Task<BlobID[]> RetrieveAsync(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return Task.FromResult(retrieve(cmd, dr, expectedCapacity));
        }

        public BlobID[] Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            return retrieve(cmd, dr, expectedCapacity);
        }

        private BlobID[] retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            List<BlobID> ids = new List<BlobID>(expectedCapacity);
            while (dr.Read())
            {
                ids.Add((BlobID)dr.GetSqlBinary(0).Value);
            }
            return ids.ToArray();
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
