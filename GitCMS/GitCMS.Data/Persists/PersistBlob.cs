using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Persists
{
    public sealed class PersistBlob : IDataOperation<Blob>
    {
        private Blob _bl;

        public PersistBlob(Blob bl)
        {
            this._bl = bl;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            var cmdText = String.Format(
                @"INSERT INTO {0} ({1}) VALUES ({2})",
                Tables.TableName_Blob,
                Tables.TablePKs_Blob.Concat(Tables.ColumnNames_Blob).NameList(),
                Tables.TablePKs_Blob.Concat(Tables.ColumnNames_Blob).ParameterList()
            );

            var cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@blobid", new SqlBinary((byte[])_bl.ID));
            cmd.AddInParameter("@contents", new SqlBinary(_bl.Contents));
            return cmd;
        }

        public Blob Return(SqlCommand cmd, int rowsAffected)
        {
            return this._bl;
        }
    }
}
