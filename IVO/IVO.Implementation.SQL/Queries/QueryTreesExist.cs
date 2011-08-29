using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using System.Collections.Generic;

namespace IVO.Implementation.SQL.Queries
{
    public sealed class QueryTreesExist : ISimpleDataQuery<TreeID>
    {
        private TreeID[] _ids;

        public QueryTreesExist(params TreeID[] ids)
        {
            this._ids = ids;
        }

        public QueryTreesExist(IEnumerable<TreeID> ids)
        {
            this._ids = ids.ToArray();
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Tree.Single();
            string cmdText = String.Format(
                @"SELECT [{0}] FROM {1}{2} WHERE [{0}] IN ({3})",
                pkName,
                Tables.TableName_Tree,
                Tables.TableFromHint_Tree,
                _ids.Select(id => String.Format("0x{0}", id.ToString())).CommaList()
            );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            return cmd;
        }

        public TreeID Project(SqlCommand cmd, SqlDataReader dr)
        {
            TreeID id = (TreeID)dr.GetSqlBinary(0).Value;
            return id;
        }
    }
}
