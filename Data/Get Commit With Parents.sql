USE [GitCMS];
GO

EXEC sp_executesql N'SELECT @commitid = [cm].[commitid] FROM [dbo].[Commit]cm WITH (NOLOCK)
JOIN [dbo].[Ref]rf WITH (NOLOCK) ON cm.[commitid] = rf.[commitid]
WHERE rf.[name] = @refname;
SELECT [cm].[commitid], [cm].[treeid],[cm].[committer],[cm].[date_committed],[cm].[message] FROM [dbo].[Commit]cm WITH (NOLOCK) WHERE cm.[commitid] = @commitid;
SELECT [parent_commitid] FROM [dbo].[CommitParent] WHERE [commitid] = @commitid;',
    N'@commitid binary(20) OUTPUT,@refname nvarchar(20)',
      @commitid = NULL,
      @refname = N'HEAD'