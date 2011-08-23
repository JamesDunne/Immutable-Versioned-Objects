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
            "contents",
        };

        #endregion

        #region Tree table

        internal const string TableName_Tree = "[dbo].[Tree]";
        internal const string TableFromHint_Tree = @" WITH (NOLOCK)";
        internal static readonly string[] TablePKs_Tree = new string[1] { "treeid" };
        internal static readonly string[] ColumnNames_Tree = new string[]
        {
        };

        #endregion

        #region TreeTree table

        internal const string TableName_TreeTree = "[dbo].[TreeTree]";
        internal const string TableFromHint_TreeTree = @" WITH (NOLOCK)";
        internal static readonly string[] TablePKs_TreeTree = new string[0];
        internal static readonly string[] ColumnNames_TreeTree = new string[]
        {
            "treeid",
            "linked_treeid",
            "name",
        };

        #endregion

        #region TreeBlob table

        internal const string TableName_TreeBlob = "[dbo].[TreeBlob]";
        internal const string TableFromHint_TreeBlob = @" WITH (NOLOCK)";
        internal static readonly string[] TablePKs_TreeBlob = new string[0];
        internal static readonly string[] ColumnNames_TreeBlob = new string[]
        {
            "treeid",
            "linked_blobid",
            "name",
        };

        #endregion
    }
}
