CREATE TABLE [dbo].[ProviderStarsSummary](
	[Ukprn] [bigint] NOT NULL,
	[ReviewCount] [int] NOT NULL,
	[Stars] [int] NOT NULL,
	[TimePeriod] NVARCHAR(50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Ukprn] ASC
))

GO
