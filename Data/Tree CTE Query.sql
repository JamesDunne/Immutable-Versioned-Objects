USE [GitCMS];
GO

DECLARE @treeid binary(20);

SET @treeid = 0xa1fe342751e09fda968cfd0f1a1755e386f494f8;
--SET @treeid = 0x85cfe62db1cedba5e7c3a056c636c1df8557a305;
--SET @treeid = 0x837166ec9e3168a3c11fc8eb461dac014e153ed0;
--SET @treeid = 0xF0452FB5E6C849F63AD02745F4E0D25461D53F24;
--SET @treeid = 0x4E8DF76F61C13254D2033E4EE1D22FFC460425F3;

;WITH Trees AS (
    SELECT      CONVERT(binary(20), NULL) AS treeid, tr.treeid AS linked_treeid, CONVERT(nvarchar(128), NULL) COLLATE SQL_Latin1_General_CP1_CS_AS AS name
    FROM        [dbo].[Tree] tr
    WHERE       tr.treeid = @treeid
    UNION ALL
    SELECT      tr.treeid, tr.linked_treeid, tr.name
    FROM        [dbo].[TreeTree] tr
    JOIN        Trees parent ON parent.linked_treeid = tr.treeid
)
SELECT  [tr].[treeid] AS tr_treeid
       ,[tr].[linked_treeid] AS tr_linked_treeid
       ,[tr].[name] AS tr_name
       ,[bl].[linked_blobid] AS trbl_linked_blobid
       ,[bl].[name] AS trbl_name
FROM    Trees tr
LEFT JOIN [dbo].[TreeBlob] bl ON bl.treeid = tr.linked_treeid
