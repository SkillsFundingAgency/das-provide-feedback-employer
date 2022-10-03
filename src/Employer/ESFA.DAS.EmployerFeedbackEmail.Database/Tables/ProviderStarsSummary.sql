CREATE TABLE [dbo].[ProviderStarsSummary](
	[Ukprn] [bigint] NOT NULL,
	[ReviewCount] [int] NOT NULL,
	[Stars] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Ukprn] ASC
))

GO

ALTER TABLE [dbo].[ProviderStarsSummary] ADD  DEFAULT ((0)) FOR [ReviewCount]
GO
