using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Queries
{
    public class QueryCommit : DataQuery<Commit>
    {
        private CommitID _id;

        public QueryCommit(CommitID id)
        {
            this._id = id;
        }

        public override SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
                @"SELECT {0} FROM {1}{2}{3} WHERE commitid = @id",
                Tables.TablePKs_Commit.Concat(Tables.ColumnNames_Commit).NameList(),
                Tables.TableName_Commit,
                "", // no alias
                Tables.TableFromHint_Commit
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@id", new SqlBinary((byte[])_id));
            return cmd;
        }

        public override Commit Project(SqlDataReader dr)
        {
            throw new NotImplementedException();
        }
    }
}
