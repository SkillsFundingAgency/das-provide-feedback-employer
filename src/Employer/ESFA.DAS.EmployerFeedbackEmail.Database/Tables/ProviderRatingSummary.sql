CREATE TABLE [dbo].[ProviderRatingSummary](
	[Ukprn] [bigint] NOT NULL,
	[Rating] [nvarchar](20) NOT NULL,
	[RatingCount] [int] NOT NULL,
	[UpdatedOn] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Ukprn] ASC,
	[Rating] ASC
))

GO
