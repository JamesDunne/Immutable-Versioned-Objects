using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlTypes;

namespace IVO.Implementation.SQL
{
    internal static class Helpers
    {
        internal static string NameCommaList(this IEnumerable<string> columnNames)
        {
            return String.Join(",", columnNames.Select(c => String.Concat("[", c, "]")).ToArray());
        }

        internal static string NameCommaList(this IEnumerable<string> columnNames, string tableAlias)
        {
            return String.Join(",", columnNames.Select(c => String.Concat("[", tableAlias, "].[", c, "]")).ToArray());
        }

        internal static string NameCommaListAs(this IEnumerable<string> columnNames, string tableAlias, string prefix)
        {
            return String.Join(",", columnNames.Select(c => String.Concat("[", tableAlias, "].[", c, "] AS [", prefix, c, "]")).ToArray());
        }

        internal static string NameCustomList(this IEnumerable<string> columnNames, string delim, Func<string, string> formatColumn)
        {
            return String.Join(delim, columnNames.Select(c => formatColumn(c)).ToArray());
        }

        internal static string ParameterCommaList(this IEnumerable<string> columnNames, string prefix = null, string suffix = null)
        {
            string realPrefix = prefix ?? String.Empty;
            string realSuffix = suffix ?? String.Empty;
            return String.Join(",", columnNames.Select(c => String.Concat("@", realPrefix, c, realSuffix)).ToArray());
        }

        internal static string ParameterCommaListAs(this IEnumerable<string> columnNames, string prefix = null, string suffix = null)
        {
            string realPrefix = prefix ?? String.Empty;
            string realSuffix = suffix ?? String.Empty;
            return String.Join(",", columnNames.Select(c => String.Concat("@", realPrefix, c, realSuffix, " AS [", realPrefix, c, realSuffix, "]")).ToArray());
        }

        internal static string CommaList(this IEnumerable<string> columnNames)
        {
            return String.Join(",", columnNames.ToArray());
        }

        internal static SqlParameter AddOutParameter(this SqlCommand cmd, string name, SqlDbType type, int size)
        {
            var prm = cmd.CreateParameter();
            prm.Direction = ParameterDirection.Output;
            prm.ParameterName = name;
            prm.SqlDbType = type;
            prm.Size = size;
            cmd.Parameters.Add(prm);
            return prm;
        }

        internal static SqlParameter AddInParameter(this SqlCommand cmd, string name, SqlBinary value, int size = 20)
        {
            var prm = cmd.CreateParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = SqlDbType.Binary;
            prm.SqlValue = value;
            prm.Size = size;
            cmd.Parameters.Add(prm);
            return prm;
        }

        internal static SqlParameter AddInParameterExactSized(this SqlCommand cmd, string name, SqlBinary value)
        {
            var prm = cmd.CreateParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = SqlDbType.Binary;
            prm.SqlValue = value;
            prm.Size = value.Length;
            cmd.Parameters.Add(prm);
            return prm;
        }

        internal static SqlParameter AddInParameter(this SqlCommand cmd, string name, SqlString value, int size = 128)
        {
            var prm = cmd.CreateParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = SqlDbType.NVarChar;
            prm.SqlValue = value;
            cmd.Parameters.Add(prm);
            return prm;
        }

        internal static SqlParameter AddInParameter(this SqlCommand cmd, string name, SqlInt32 value)
        {
            var prm = cmd.CreateParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = SqlDbType.Int;
            prm.SqlValue = value;
            cmd.Parameters.Add(prm);
            return prm;
        }

        internal static SqlParameter AddInParameter(this SqlCommand cmd, string name, DateTimeOffset value)
        {
            var prm = cmd.CreateParameter();
            prm.Direction = ParameterDirection.Input;
            prm.ParameterName = name;
            prm.SqlDbType = SqlDbType.DateTimeOffset;
            prm.SqlValue = value;
            cmd.Parameters.Add(prm);
            return prm;
        }
    }
}
