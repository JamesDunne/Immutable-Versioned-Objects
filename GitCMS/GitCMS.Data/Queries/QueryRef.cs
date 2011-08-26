using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Queries
{
    public sealed class QueryRef : ISimpleDataQuery<Ref>
    {
        private string _name;

        public QueryRef(string name)
        {
            this._name = name;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
                @"SELECT {0} FROM {1}{2}{3} WHERE [name] = @name",
                Tables.ColumnNames_Ref.NameList(),
                Tables.TableName_Ref,
                "", // no alias
                Tables.TableFromHint_Ref
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@name", new SqlString(this._name));
            return cmd;
        }

        public Ref Project(SqlDataReader dr)
        {
            Ref.Builder b = new Ref.Builder(
                pName:      dr.GetSqlString(0).Value,
                pCommitID:  (CommitID)dr.GetSqlBinary(1).Value
            );

            Ref rf = b;
            return rf;
        }
    }
}
