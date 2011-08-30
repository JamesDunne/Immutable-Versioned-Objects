using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;

namespace IVO.Implementation.SQL.Persists
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
            // MERGE .. WHEN NOT MATCHED is used in SQL2008 to avoid primary key constraint race condition
            // when INSERTing records with duplicate SHA-1 ids.
            var cmdText = String.Format(
@"SET NOCOUNT, XACT_ABORT ON;
MERGE {0} WITH (HOLDLOCK) AS curr_blob
USING (SELECT {3} AS {1}) AS new_blob ON curr_blob.{1} = new_blob.{1}
WHEN NOT MATCHED THEN INSERT ({2}) VALUES ({4});",
                Tables.TableName_Blob,  // 0
                Tables.TablePKs_Blob.Single(),  // 1
                Tables.TablePKs_Blob.Concat(Tables.ColumnNames_Blob).NameCommaList(),    // 2
                "@" + Tables.TablePKs_Blob.Single(),    // 3
                Tables.TablePKs_Blob.Concat(Tables.ColumnNames_Blob).ParameterCommaList()    // 4
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
