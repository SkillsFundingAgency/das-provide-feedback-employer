CREATE PROCEDURE [dbo].[UpsertProviders]
	@ProvidersDt ProviderTemplate READONLY
AS
	MERGE [dbo].[Providers] AS [Target]
	USING @ProvidersDt AS [Source]
	ON [Target].Ukprn = [Source].Ukprn
	WHEN MATCHED THEN UPDATE SET [Target].ProviderName = [Source].ProviderName, [Target].IsActive = 1
	WHEN NOT MATCHED THEN INSERT (Ukprn, ProviderName, IsActive)
	VALUES([Source].Ukprn, [Source].ProviderName, 1);