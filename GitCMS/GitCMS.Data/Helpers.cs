using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlTypes;

namespace GitCMS.Data
{
    internal static class Helpers
    {
        internal static string NameList(this IEnumerable<string> columnNames)
        {
            return String.Join(",", columnNames.Select(c => String.Concat("[", c, "]")).ToArray());
        }

        internal static string NameList(this IEnumerable<string> columnNames, string tableAlias)
        {
            return String.Join(",", columnNames.Select(c => String.Concat("[", tableAlias, "].[", c, "] AS [", tableAlias, "_", c, "]")).ToArray());
        }

        internal static string ParameterList(this IEnumerable<string> columnNames)
        {
            return String.Join(",", columnNames.Select(c => String.Concat("@", c)).ToArray());
        }

        internal static string CommaList(this IEnumerable<string> columnNames)
        {
            return String.Join(",", columnNames.ToArray());
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
