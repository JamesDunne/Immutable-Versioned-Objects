USE [IVO];
GO

DECLARE @rootid BINARY(20);
DECLARE @treeid BINARY(20);
DECLARE @path NVARCHAR(256);

SET @rootid = 0x450C56729802411D02A62D6688744CFBD6169953;
SET @path = N'/src/Persists/';

;WITH Trees AS (
    SELECT      CONVERT(binary(20), NULL) AS treeid, tr.treeid AS linked_treeid, CONVERT(nvarchar(128), NULL) COLLATE SQL_Latin1_General_CP1_CS_AS AS name, CONVERT(NVARCHAR(256), N'/') COLLATE SQL_Latin1_General_CP1_CS_AS AS [path]
    FROM        [dbo].[Tree] tr
    WHERE       tr.treeid = @rootid
    UNION ALL
    SELECT      tr.treeid, tr.linked_treeid, tr.name, CONVERT(NVARCHAR(256), parent.[path] + tr.name + N'/') COLLATE SQL_Latin1_General_CP1_CS_AS AS [path]
    FROM        [dbo].[TreeTree] tr
    JOIN        Trees parent ON parent.linked_treeid = tr.treeid
)
SELECT  @treeid = [tr].linked_treeid
FROM    Trees tr
WHERE   tr.[path] = @path

SELECT @treeid;
