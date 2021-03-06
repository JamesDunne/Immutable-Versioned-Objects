﻿using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Asynq;
using IVO.Definition.Models;
using IVO.Definition.Errors;

namespace IVO.Implementation.SQL.Persists
{
    public sealed class DestroyTagByName : IDataOperation<Errorable<TagID>>
    {
        private TagName _tagName;

        public DestroyTagByName(TagName tagName)
        {
            this._tagName = tagName;
        }

        public SqlCommand ConstructCommand(SqlConnection cn)
        {
            string pkName = Tables.TablePKs_Tag.Single();
            var cmdText = String.Format(
                @"SELECT @{1} = [{1}] FROM {0} WHERE [name] = @tagname;
IF (@{1} IS NOT NULL)
BEGIN
    DELETE FROM {0} WHERE [{1}] = @{1};
END",
                Tables.TableName_Tag,
                pkName
            );

            var cmd = new SqlCommand(cmdText, cn);
            cmd.AddOutParameter("@tagid", System.Data.SqlDbType.Binary, 20);
            cmd.AddInParameter("@tagname", new SqlString(this._tagName.ToString()));
            return cmd;
        }

        public Errorable<TagID> Return(SqlCommand cmd, int rowsAffected)
        {
            SqlBinary tagId = (SqlBinary)cmd.Parameters["@tagid"].SqlValue;

            if (tagId.IsNull) return new TagNameDoesNotExistError(this._tagName);

            return (TagID) tagId.Value;
        }
    }
}
