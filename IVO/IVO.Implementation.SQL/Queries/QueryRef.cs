using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using System.Data;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryRef : ISimpleDataQuery<Ref>
    {
        private RefName _name;

        public QueryRef(RefName name)
        {
            this._name = name;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string cmdText = String.Format(
                @"SELECT {0} FROM {1}{2}{3} WHERE [name] = @name",
                Tables.ColumnNames_Ref.NameCommaList(),
                Tables.TableName_Ref,
                "", // no alias
                Tables.TableFromHint_Ref
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@name", new SqlString(this._name.ToString()));
            return cmd;
        }

        public Ref Project(SqlCommand cmd, SqlDataReader dr)
        {
            Ref.Builder b = new Ref.Builder(
                pName:      (RefName) dr.GetSqlString(0).Value,
                pCommitID:  (CommitID)dr.GetSqlBinary(1).Value
            );

            Ref rf = b;
            return rf;
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
