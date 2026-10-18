IF OBJECT_ID(N'[dbo].[LegacyIdMap]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[LegacyIdMap]
    (
        [TableName] sysname NOT NULL,
        [LegacyId]  int     NOT NULL,
        [NewId]     int     NOT NULL,
        CONSTRAINT [PK_LegacyIdMap] PRIMARY KEY ([TableName], [LegacyId])
    );
END;
