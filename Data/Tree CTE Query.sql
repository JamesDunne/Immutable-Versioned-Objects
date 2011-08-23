DECLARE @root_treeid varbinary(20);

SET @root_treeid = 0x837166ec9e3168a3c11fc8eb461dac014e153ed0;
--SET @root_treeid = 0xF0452FB5E6C849F63AD02745F4E0D25461D53F24;
--SET @root_treeid = 0x4E8DF76F61C13254D2033E4EE1D22FFC460425F3;

;WITH Trees AS (
    SELECT      CONVERT(binary(20), NULL) AS treeid, tr.treeid AS linked_treeid, CONVERT(nvarchar(128), NULL) AS name
    FROM        [dbo].[Tree] tr
    WHERE       LEFT(tr.treeid, LEN(@root_treeid)) = @root_treeid
    UNION ALL
    SELECT      tr.treeid, tr.linked_treeid, tr.name
    FROM        [dbo].[TreeTree] tr
    JOIN        Trees parent ON parent.linked_treeid = tr.treeid
)
SELECT  tr.treeid
       ,tr.linked_treeid
       ,tr.name AS tree_name
       ,bl.linked_blobid
       ,bl.name AS blob_name
FROM    Trees tr
LEFT JOIN [dbo].[TreeBlob] bl ON bl.treeid = tr.linked_treeid
