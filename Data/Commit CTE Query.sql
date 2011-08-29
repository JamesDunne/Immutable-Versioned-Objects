USE [IVO];
GO

DECLARE @commitid binary(20);

SET @commitid = 0x30ede38fc7d0b28dcbfa0a51b73d113241e53ad3;

;WITH Commits AS (
    SELECT      CAST(NULL AS binary(20)) AS commitid, cm.commitid AS parent_commitid, cm.treeid, cm.committer, cm.date_committed, cm.[message], 1 AS reclvl
    FROM        [dbo].[Commit] cm
    WHERE       cm.commitid = @commitid
    UNION ALL
    SELECT      cp.commitid, cp.parent_commitid, cm.treeid, cm.committer, cm.date_committed, cm.[message], 1 + parent.reclvl AS reclvl
    FROM        [dbo].[CommitParent] cp
    JOIN        [dbo].[Commit] cm ON cp.parent_commitid = cm.commitid
    JOIN        Commits parent ON parent.parent_commitid = cp.commitid
)
SELECT  [cm].[commitid]
       ,[cm].[parent_commitid]
       , cm.treeid, cm.committer, cm.date_committed, cm.[message]
FROM    Commits cm
WHERE   cm.reclvl <= 10