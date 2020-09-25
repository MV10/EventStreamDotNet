
-- Feel free to customize the database, table, and index names. The database
-- name and table names are provided in the library's DatabaseConfiguration
-- object. The repository's demo project expects the names shown here as
-- defined in the demo config file.

-- The unique id (assigned to the root object of the domain data model) is
-- sized here at 36 characters to accomodate the default string formatting
-- of a .NET Guid.NewGuid() structure (which includes the digits and dashes
-- but excludes the enclosing braces). You can change the size, but it must
-- always be called Id and must always be an NVARCHAR and a C# string.

-- The Timestamp is sized at 33 characters to accomodate the DateTimeOffset
-- "O" string format. Example: 2019-11-27T05:41:12.1320053-05:00

USE [EventStreamDotNet]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- No clustered PK, all reads are by specific Id ordered by ETag
CREATE TABLE [dbo].[EventStreamDeltaLog] (
    [Id]         NVARCHAR (36)  NOT NULL, -- indexes don't allow MAX
	[ETag]       BIGINT         NOT NULL,
    [Timestamp]  NCHAR (33)     NOT NULL,
    [EventType]  NVARCHAR (MAX) NOT NULL,
    [Payload]    NVARCHAR (MAX) NOT NULL
);
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_EventStreamDeltaLog ON
[dbo].[EventStreamDeltaLog] ([Id] ASC, [ETag] ASC);
GO

-- No clustered PK, all reads are for a specific Id
CREATE TABLE [dbo].[DomainModelSnapshot] (
    [Id]         NVARCHAR (36)  NOT NULL,
	[ETag]       BIGINT         NOT NULL,
    [Snapshot]   NVARCHAR (MAX) NOT NULL
);
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_DomainModelSnapshot ON
[dbo].[DomainModelSnapshot] ([Id] ASC);
GO


/* 

Cut and paste to a query window to reset your database for testing:

use EventStreamDotNet;
select getdate(); -- remind yourself when you last reset it
truncate table EventStreamDeltaLog;
truncate table DomainModelSnapshot;

*/