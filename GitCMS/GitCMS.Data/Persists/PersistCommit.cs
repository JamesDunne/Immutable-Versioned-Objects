using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Persists
{
    public sealed class PersistCommit : IDataOperation
    {
        private Commit _cm;

        public PersistCommit(Commit cm)
        {
            this._cm = cm;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            var cmdText = String.Format(
                @"INSERT INTO {0} ({1}) VALUES ({2})",
                Tables.TableName_Commit,
                Tables.TablePKs_Commit.Concat(Tables.ColumnNames_Commit).NameList(),
                Tables.TablePKs_Commit.Concat(Tables.ColumnNames_Commit).ParameterList()
            );

            var cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@commitid", new SqlBinary((byte[])_cm.ID));
            cmd.AddInParameter("@treeid", new SqlBinary((byte[])_cm.TreeID));
            cmd.AddInParameter("@committer", new SqlString(_cm.Committer));
            cmd.AddInParameter("@author", new SqlString(_cm.Author));
            cmd.AddInParameter("@date_committed", new SqlDateTime(_cm.DateCommitted.UtcDateTime));
            cmd.AddInParameter("@message", new SqlString(_cm.Message));
            return cmd;
        }
    }
}
