using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using System.Text;

namespace IVO.Implementation.SQL.Persists
{
    /// <summary>
    /// Persists a single Tree instance with its TreeTree and TreeBlob records.
    /// Assumptions:
    /// <list type="">
    /// <item>Tree and its TreeTree and TreeBlob records do not exist per TreeID.</item>
    /// <item>All TreeIDs referenced are persisted.</item>
    /// <item>All BlobIDs referenced are persisted.</item>
    /// </list>
    /// </summary>
    public sealed class PersistTree : IDataOperation<Tree>
    {
        private Tree _tr;

        public PersistTree(Tree tr)
        {
            this._tr = tr;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            StringBuilder sbCmd = new StringBuilder();

            sbCmd.AppendLine(@"SET NOCOUNT, XACT_ABORT ON;");
            
            // MERGE .. WHEN NOT MATCHED is used in SQL2008 to avoid primary key constraint race condition
            // when INSERTing records with duplicate SHA-1 ids.
            sbCmd.AppendFormat(
@"MERGE {0} WITH (HOLDLOCK) AS ct
USING (SELECT {3} AS {1}) AS nt ON ct.{1} = nt.{1}
WHEN NOT MATCHED THEN INSERT ({2}) VALUES ({4});
",
                Tables.TableName_Tree,  // 0
                Tables.TablePKs_Tree.Single(),  // 1
                Tables.TablePKs_Tree.Concat(Tables.ColumnNames_Tree).NameCommaList(),    // 2
                "@" + Tables.TablePKs_Tree.Single(),    // 3
                Tables.TablePKs_Tree.Concat(Tables.ColumnNames_Tree).ParameterCommaList()    // 4
            );

            var trtrCols = Tables.ColumnNames_TreeTree.Except(new string[1] { "treeid" }).ToArray();
            for (int i = 0; i < _tr.Trees.Length; ++i)
            {
                sbCmd.AppendFormat(
@"MERGE {0} WITH (HOLDLOCK) AS ct
USING (SELECT {1}) AS nt ON {2}
WHEN NOT MATCHED THEN INSERT ({3}) VALUES ({4});
",
                    Tables.TableName_TreeTree,
                    trtrCols.NameCustomList(",", c => String.Format("@trtr_{0}{1} AS [{0}]", c, i.ToString())),
                    trtrCols.NameCustomList(" AND ", c => String.Format("(ct.[{0}] = nt.[{0}])", c)),
                    Tables.ColumnNames_TreeTree.NameCommaList(),
                    "@treeid," + trtrCols.NameCustomList(",", c => String.Format("@trtr_{0}{1}", c, i.ToString()))
                );
            }

            var trblCols = Tables.ColumnNames_TreeBlob.Except(new string[1] { "treeid" }).ToArray();
            for (int i = 0; i < _tr.Blobs.Length; ++i)
            {
                sbCmd.AppendFormat(
@"MERGE {0} WITH (HOLDLOCK) AS ct
USING (SELECT {1}) AS nt ON {2}
WHEN NOT MATCHED THEN INSERT ({3}) VALUES ({4});
",
                    Tables.TableName_TreeBlob,
                    trblCols.NameCustomList(",", c => String.Format("@trbl_{0}{1} AS [{0}]", c, i.ToString())),
                    trblCols.NameCustomList(" AND ", c => String.Format("(ct.[{0}] = nt.[{0}])", c)),
                    Tables.ColumnNames_TreeBlob.NameCommaList(),
                    "@treeid," + trblCols.NameCustomList(",", c => String.Format("@trbl_{0}{1}", c, i.ToString()))
                );
            }

            string cmdText = sbCmd.ToString();
            var cmd = new SqlCommand(cmdText, cn);

            // Add parameters:
            cmd.AddInParameter("@treeid", new SqlBinary((byte[])_tr.ID));
            for (int i = 0; i < _tr.Trees.Length; ++i)
            {
                cmd.AddInParameter(String.Format("@trtr_linked_treeid{0}", i.ToString()), new SqlBinary((byte[])_tr.Trees[i].TreeID));
                cmd.AddInParameter(String.Format("@trtr_name{0}", i.ToString()), new SqlString(_tr.Trees[i].Name));
            }
            for (int i = 0; i < _tr.Blobs.Length; ++i)
            {
                cmd.AddInParameter(String.Format("@trbl_linked_blobid{0}", i.ToString()), new SqlBinary((byte[])_tr.Blobs[i].BlobID));
                cmd.AddInParameter(String.Format("@trbl_name{0}", i.ToString()), new SqlString(_tr.Blobs[i].Name));
            }

            return cmd;
        }

        public Tree Return(SqlCommand cmd, int rowsAffected)
        {
            return this._tr;
        }
    }
}
