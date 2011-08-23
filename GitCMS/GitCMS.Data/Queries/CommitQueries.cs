using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using GitCMS.Definition.Models;
using Asynq;

namespace GitCMS.Data.Queries
{
    public class CommitQuery : DataQuery<Commit>
    {
        private CommitID _id;

        public CommitQuery(CommitID id)
        {
            this._id = id;
        }

        public override SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
                @"SELECT {0} FROM {1}{2}{3} WHERE commitid = @id",
                Helpers.ColumnNames_Commit.NameList(),
                Helpers.TableName_Commit,
                "", // no alias
                Helpers.FromTableHint_Commit
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
