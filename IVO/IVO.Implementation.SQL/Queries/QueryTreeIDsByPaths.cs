using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Exceptions;
using System.Data;
using System.Collections.ObjectModel;

namespace IVO.Implementation.SQL.Queries
{
    public class QueryTreeIDsByPaths : IComplexDataQuery<ReadOnlyCollection<TreeIDPathMapping>>
    {
        private CanonicalTreePath[] _paths;
        private TreeID _rootid;

        public QueryTreeIDsByPaths(TreeID rootid, params CanonicalTreePath[] paths)
        {
            this._rootid = rootid;
            this._paths = paths;
        }

        private string safeSqlLiteral(string inputSQL)
        {
            return inputSQL.Replace("'", "''");
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Tree.Single();
            string cmdText =
                String.Format(
@";WITH rec AS (
    SELECT      CONVERT(binary(20), NULL) AS treeid, tr.treeid AS linked_treeid, CONVERT(nvarchar(128), NULL) COLLATE SQL_Latin1_General_CP1_CS_AS AS name, CONVERT(NVARCHAR(256), N'/') COLLATE SQL_Latin1_General_CP1_CS_AS AS [path]
    FROM        [dbo].[Tree] tr
    WHERE       tr.treeid = @rootid
    UNION ALL
    SELECT      tr.treeid, tr.linked_treeid, tr.name, CONVERT(NVARCHAR(256), parent.[path] + tr.name + N'/') COLLATE SQL_Latin1_General_CP1_CS_AS AS [path]
    FROM        [dbo].[TreeTree] tr
    JOIN        rec parent ON parent.linked_treeid = tr.treeid
)
SELECT  [tr].linked_treeid AS [treeid], tr.[path] FROM rec tr WHERE tr.[path] IN ({0});",
                    String.Join(",", this._paths.SelectAsArray(path => "N'" + safeSqlLiteral(path.ToString()) + "'"))
                );

            SqlCommand cmd = new SqlCommand(cmdText, cn);
            cmd.AddInParameter("@rootid", new SqlBinary((byte[])this._rootid));
            return cmd;
        }

        public ReadOnlyCollection<TreeIDPathMapping> Retrieve(SqlCommand cmd, SqlDataReader dr, int expectedCapacity = 10)
        {
            List<TreeIDPathMapping> mappings = new List<TreeIDPathMapping>();

            // Read the TreeTreeReferences:
            while (dr.Read())
            {
                SqlBinary bid = dr.GetSqlBinary(0);

                TreeID? id = bid.IsNull ? (TreeID?)null : (TreeID?)bid.Value;
                string path = dr.GetSqlString(1).Value;

                mappings.Add(new TreeIDPathMapping(new TreeTreePath(_rootid, (CanonicalTreePath)path), id));
            }

            return new ReadOnlyCollection<TreeIDPathMapping>(mappings);
        }

        public CommandBehavior GetCustomCommandBehaviors(SqlConnection cn, SqlCommand cmd)
        {
            return CommandBehavior.Default;
        }
    }
}
