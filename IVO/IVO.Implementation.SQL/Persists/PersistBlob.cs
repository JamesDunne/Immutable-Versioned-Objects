﻿using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;

namespace IVO.Implementation.SQL.Persists
{
    public sealed class PersistBlob : IDataOperation<PersistingBlob>
    {
        private PersistingBlob _bl;

        public PersistBlob(PersistingBlob bl)
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

            BlobID blid;
            if (_bl.ID.HasValue)
                blid = _bl.ID.Value;
            else
            {
                // TODO: asynchrony!
                blid = _bl.ComputeID();
            }

            cmd.AddInParameter("@blobid", new SqlBinary((byte[])blid));

            // Open a new stream of the source contents to upload to the database:
            using (var sr = _bl.GetNewStream())
            {
                byte[] dum = new byte[sr.Length];

                // TODO: chunked xactional update to [contents] in multiples of 8040 bytes.
                sr.Read(dum, 0, dum.Length);

                cmd.AddInParameter("@contents", new SqlBinary(dum), size: dum.Length);
            }
            return cmd;
        }

        public PersistingBlob Return(SqlCommand cmd, int rowsAffected)
        {
            return this._bl;
        }
    }
}
