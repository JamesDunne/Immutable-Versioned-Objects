using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCMS.Data
{
    internal static class Tables
    {
        #region Commit table

        internal const string TableName_Commit = "[dbo].[Commit]";
        internal const string TableFromHint_Commit = @" WITH (NOLOCK)";
        internal static readonly string[] TablePKs_Commit = new string[1] { "commitid" };
        internal static readonly string[] ColumnNames_Commit = new string[]
        {
            "treeid",
            "committer",
            "author",
            "date_committed",
            "message",
        };

        #endregion

        #region Blob table
        
        internal const string TableName_Blob = "[dbo].[Blob]";
        internal const string TableFromHint_Blob = @" WITH (NOLOCK)";
        internal static readonly string[] TablePKs_Blob = new string[1] { "blobid" };
        internal static readonly string[] ColumnNames_Blob = new string[]
        {
            "contents"
        };

        #endregion
    }
}
