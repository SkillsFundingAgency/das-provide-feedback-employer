CREATE TABLE [dbo].[ProviderStarsSummary](
	[Ukprn] [bigint] NOT NULL,
	[ReviewCount] [int] NOT NULL,
	[Stars] [int] NOT NULL,
	[TimePeriod] NVARCHAR(50) NOT NULL DEFAULT 'All',
PRIMARY KEY CLUSTERED 
(
	[Ukprn] ASC,
	[TimePeriod] ASC
))

GO
