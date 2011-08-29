using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using System.Collections.Generic;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryBlobsExist : ISimpleDataQuery<BlobID>
    {
        private BlobID[] _ids;

        public QueryBlobsExist(params BlobID[] ids)
        {
            this._ids = ids;
        }

        public QueryBlobsExist(IEnumerable<BlobID> ids)
        {
            this._ids = ids.ToArray();
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Blob.Single();
            string cmdText = String.Format(
                @"SELECT [{0}] FROM {1}{2} WHERE [{0}] IN ({3})",
                pkName,
                Tables.TableName_Blob,
                Tables.TableFromHint_Blob,
                _ids.Select(id => String.Format("0x{0}", id.ToString())).CommaList()
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            return cmd;
        }

        public BlobID Project(SqlDataReader dr)
        {
            BlobID id = (BlobID) dr.GetSqlBinary(0).Value;

            return id;
        }
    }
}
