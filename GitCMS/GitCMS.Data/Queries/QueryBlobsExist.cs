using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Queries
{
    public sealed class QueryBlobsExist : DataQuery<BlobID>
    {
        private BlobID[] _ids;

        public QueryBlobsExist(params BlobID[] ids)
        {
            this._ids = ids;
        }

        public override SqlCommand ConstructCommand(SqlConnection cn)
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

        public override BlobID Project(SqlDataReader dr)
        {
            BlobID id = (BlobID) dr.GetSqlBinary(0).Value;

            return id;
        }
    }
}
