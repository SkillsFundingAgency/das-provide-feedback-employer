CREATE TABLE [dbo].[ProviderRatingSummary](
	[Ukprn] [bigint] NOT NULL,
	[Rating] [nvarchar](20) NOT NULL,
	[RatingCount] [int] NOT NULL,
	[TimePeriod] NVARCHAR(50) NOT NULL DEFAULT 'All',
	[UpdatedOn] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Ukprn] ASC,
	[Rating] ASC,
	[TimePeriod] ASC
))

GO
