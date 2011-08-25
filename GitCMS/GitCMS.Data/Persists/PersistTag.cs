using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Persists
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
                Tables.TablePKs_Tag.Concat(Tables.ColumnNames_Tag).NameList(),
                Tables.TablePKs_Tag.Concat(Tables.ColumnNames_Tag).ParameterList()
            );

            var cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@tagid", new SqlBinary((byte[])_tg.ID));
            cmd.AddInParameter("@commitid", new SqlBinary((byte[])_tg.CommitID));
            // TODO: add timestamp?
            cmd.AddInParameter("@message", new SqlString(_tg.Message));
            return cmd;
        }

        public Tag Return(int rowsAffected)
        {
            return this._tg;
        }
    }
}
