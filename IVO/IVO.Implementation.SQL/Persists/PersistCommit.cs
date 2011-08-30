using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using System.Text;

namespace IVO.Implementation.SQL.Persists
{
    public sealed class PersistCommit : IDataOperation<Commit>
    {
        private Commit _cm;

        public PersistCommit(Commit cm)
        {
            this._cm = cm;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.AppendLine(@"BEGIN TRAN");
            sbCmd.AppendFormat(
                @"INSERT INTO {0} ({1}) VALUES ({2})",
                Tables.TableName_Commit,
                Tables.TablePKs_Commit.Concat(Tables.ColumnNames_Commit).NameCommaList(),
                Tables.TablePKs_Commit.Concat(Tables.ColumnNames_Commit).ParameterCommaList()
            );
            for (int i = 0; i < _cm.Parents.Length; ++i)
            {
                sbCmd.AppendFormat(@"INSERT INTO [dbo].[CommitParent] (commitid, parent_commitid) VALUES (@commitid,@pc{0})", i.ToString());
            }
            sbCmd.AppendLine(@"COMMIT TRAN");

            var cmd = new SqlCommand(sbCmd.ToString(), cn);
            cmd.AddInParameter("@commitid", new SqlBinary((byte[])_cm.ID));
            cmd.AddInParameter("@treeid", new SqlBinary((byte[])_cm.TreeID));
            cmd.AddInParameter("@committer", new SqlString(_cm.Committer), size: 512);
            cmd.AddInParameter("@date_committed", _cm.DateCommitted);
            // TODO: chunked xactional update to [message] in multiples of 8040 bytes.
            cmd.AddInParameter("@message", new SqlString(_cm.Message), size: ((_cm.Message.Length / 8040) + (_cm.Message.Length % 8040 > 0 ? 1 : 0)) * 8040);
            for (int i = 0; i < _cm.Parents.Length; ++i)
                cmd.AddInParameter("@pc" + i.ToString(), new SqlBinary((byte[])_cm.Parents[i]));
            return cmd;
        }

        public Commit Return(SqlCommand cmd, int rowsAffected)
        {
            return this._cm;
        }
    }
}
