CREATE TABLE [dbo].[Providers]
(
	[Ukprn] BIGINT NOT NULL PRIMARY KEY, 
    [ProviderName] NVARCHAR(150) NOT NULL, 
    [IsActive] BIT NOT NULL DEFAULT 0
)
