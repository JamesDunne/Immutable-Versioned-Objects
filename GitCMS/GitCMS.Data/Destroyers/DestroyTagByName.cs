using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using GitCMS.Definition.Models;

namespace GitCMS.Data.Persists
{
    public sealed class DestroyTagByName : IDataOperation<TagID>
    {
        private string _tagName;

        public DestroyTagByName(string tagName)
        {
            this._tagName = tagName;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Tag.Single();
            var cmdText = String.Format(
                @"SELECT @{1} = [{1}] FROM {0} WHERE [name] = @tagname;
DELETE FROM {0} WHERE [{1}] = @{1};",
                Tables.TableName_Tag,
                pkName
            );

            var cmd = new SqlCommand(cmdText, cn);
            cmd.AddOutParameter("@tagid", System.Data.SqlDbType.Binary, 20);
            cmd.AddInParameter("@tagname", new SqlString(_tagName));
            return cmd;
        }

        public TagID Return(SqlCommand cmd, int rowsAffected)
        {
            return (TagID) ((SqlBinary)cmd.Parameters["@tagid"].SqlValue).Value;
        }
    }
}
