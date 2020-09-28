
-- The repository's demo project simulates a simple banking system. These tables
-- are not part of EventStreamDotNet, they are specific to the demo. They are used
-- to show how the projection handlers work.

USE [EventStreamDotNet]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Driven by the SpouseChanged and SpouseRemoved domain events
CREATE TABLE [dbo].[MaritalStatusProjection] (
    [Id]         NVARCHAR (36)  NOT NULL,
	[Status]     NCHAR (7)      NOT NULL  -- MARRIED or SINGLE
);
GO

-- Driven by snapshot updates
CREATE TABLE [dbo].[ResidencyProjection] (
    [Id]         NVARCHAR (36)  NOT NULL,
	[State]      NCHAR (2)      NOT NULL
);
GO
