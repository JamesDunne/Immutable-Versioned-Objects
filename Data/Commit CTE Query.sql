USE [GitCMS];
GO

DECLARE @commitid binary(20);

SET @commitid = 0xf92b6b4432d3d624285c3994e10329453587646d;

;WITH Commits AS (
    SELECT      CONVERT(binary(20), NULL) AS commitid, cm.commitid AS parent_commitid
    FROM        [dbo].[Commit] cm
    WHERE       cm.commitid = @commitid
    UNION ALL
    SELECT      cm.commitid, cm.parent_commitid
    FROM        [dbo].[CommitParent] cm
    JOIN        Commits parent ON parent.parent_commitid = cm.commitid
)
SELECT  [cm].[commitid]
       ,[cm].[parent_commitid]
FROM    Commits cm
