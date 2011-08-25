using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;
using System.Text;

namespace GitCMS.Data.Persists
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
            sbCmd.AppendLine(@"BEGIN TRAN");
            sbCmd.AppendFormat(@"INSERT INTO {0} ({1}) VALUES ({2});",
                Tables.TableName_Tree,
                Tables.TablePKs_Tree.Concat(Tables.ColumnNames_Tree).NameList(),
                Tables.TablePKs_Tree.Concat(Tables.ColumnNames_Tree).ParameterList()
            );

            for (int i = 0; i < _tr.Trees.Length; ++i)
            {
                sbCmd.AppendFormat(@"INSERT INTO {0} ({1}) VALUES (@treeid,{2});",
                    Tables.TableName_TreeTree,
                    Tables.ColumnNames_TreeTree.NameList(),
                    Tables.ColumnNames_TreeTree.Except(new string[1] { "treeid" }).ParameterList(prefix: "trtr_", suffix: i.ToString())
                );
            }

            for (int i = 0; i < _tr.Blobs.Length; ++i)
            {
                sbCmd.AppendFormat(@"INSERT INTO {0} ({1}) VALUES (@treeid,{2});",
                    Tables.TableName_TreeBlob,
                    Tables.ColumnNames_TreeBlob.NameList(),
                    Tables.ColumnNames_TreeBlob.Except(new string[1] { "treeid" }).ParameterList(prefix: "trbl_", suffix: i.ToString())
                );
            }
            sbCmd.AppendLine(@"COMMIT TRAN");

            var cmd = new SqlCommand(sbCmd.ToString(), cn);

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

        public Tree Return(int rowsAffected)
        {
            return this._tr;
        }
    }
}
