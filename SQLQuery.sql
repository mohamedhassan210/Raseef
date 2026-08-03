-- 1. ÅםÞÇÝ ÌדםÚ ÇבÚבÇÞÇÊ דÄÞÊÇנ ÚÔÇה דÝםÔ ÌÏזב םÚÊÑÖ Úבל ÇבדÓÍ
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

-- 2. דÓÍ ‗ב ÇבÈםÇהÇÊ דה ‗ב ÇבÌÏÇזב
EXEC sp_MSforeachtable 'DELETE FROM ?';

-- 3. ÊÕÝםÑ ÚÏÇÏ ÇבÜ ID ÚÔÇה Ãם ÈםÇהÇÊ ÌÏםÏÉ ÊÈÏÃ דה ÑÞד 1
EXEC sp_MSforeachtable 'IF OBJECTPROPERTY(OBJECT_ID(''?''), ''TableHasIdentity'') = 1 DBCC CHECKIDENT (''?'', RESEED, 0)';

-- 4. ÅÚÇÏÉ ÊÔÛםב ÇבÚבÇÞÇÊ דÑÉ ÊÇהםÉ
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';