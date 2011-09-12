using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;

namespace IVO.Implementation.SQL.Persists
{
    public sealed class PersistTag : IDataOperation<Tag>
    {
        private Tag _tg;

        public PersistTag(Tag tg)
        {
            this._tg = tg;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            var cmdText = String.Format(
                @"INSERT INTO {0} ({1}) VALUES ({2})",
                Tables.TableName_Tag,
                Tables.TablePKs_Tag.Concat(Tables.ColumnNames_Tag).NameCommaList(),
                Tables.TablePKs_Tag.Concat(Tables.ColumnNames_Tag).ParameterCommaList()
            );

            var cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@tagid", new SqlBinary((byte[])this._tg.ID));
            cmd.AddInParameter("@name", new SqlString(this._tg.Name.ToString()));
            cmd.AddInParameter("@commitid", new SqlBinary((byte[])this._tg.CommitID));
            cmd.AddInParameter("@tagger", new SqlString(this._tg.Tagger));
            cmd.AddInParameter("@date_tagged", this._tg.DateTagged);
            cmd.AddInParameter("@message", new SqlString(this._tg.Message));
            return cmd;
        }

        public Tag Return(SqlCommand cmd, int rowsAffected)
        {
            return this._tg;
        }
    }
}
