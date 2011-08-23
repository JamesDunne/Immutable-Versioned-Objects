using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlTypes;

namespace GitCMS.Data.Queries
{
    internal static class Helpers
    {
        #region Commit table

        internal const string TableName_Commit = "[dbo].[Commit]";
        internal static string FromTableHint_Commit = @" WITH (NOLOCK)";
        internal static readonly string[] ColumnNames_Commit = new string[]
        {
            "commitid",
            "treeid",
            "committer",
            "author",
            "date_committed",
            "message",
        };

        #endregion

        internal static string NameList(this string[] columnNames)
        {
            return String.Join(",", columnNames.Select(c => String.Concat("[", c, "]")).ToArray());
        }

        internal static string NameList(this string[] columnNames, string tableAlias)
        {
            return String.Join(",", columnNames.Select(c => String.Concat("[", tableAlias, "].[", c, "] AS [", tableAlias, "_", c, "]")).ToArray());
        }

        internal static SqlParameter AddInParameter(this SqlCommand cmd, string name, SqlBinary value)
        {
            var prm = cmd.CreateParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = SqlDbType.Binary;
            prm.SqlValue = value;
            cmd.Parameters.Add(prm);
            return prm;
        }
    }
}
