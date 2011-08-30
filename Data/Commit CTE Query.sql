USE [IVO];
GO

DECLARE @commitid binary(20);
DECLARE @depth int;

SET @commitid = 0x30ede38fc7d0b28dcbfa0a51b73d113241e53ad3;
SET @depth = 10;

;WITH cmtree AS (
    SELECT      CAST(NULL AS binary(20)) AS commitid, cm.commitid AS parent_commitid, cm.treeid, cm.committer, cm.date_committed, cm.[message], 1 AS depth
    FROM        [dbo].[Commit] cm WITH (NOLOCK)
    WHERE       cm.commitid = @commitid
    UNION ALL
    SELECT      cp.commitid, cp.parent_commitid, cm.treeid, cm.committer, cm.date_committed, cm.[message], 1 + parent.depth AS depth
    FROM        [dbo].[CommitParent] cp WITH (NOLOCK)
    JOIN        [dbo].[Commit] cm WITH (NOLOCK) ON cp.parent_commitid = cm.commitid
    JOIN        cmtree parent ON parent.parent_commitid = cp.commitid
)
SELECT  cm.[commitid], cm.[parent_commitid], cm.treeid, cm.committer, cm.date_committed, cm.[message]
FROM    cmtree cm
WHERE   cm.depth <= @depth
