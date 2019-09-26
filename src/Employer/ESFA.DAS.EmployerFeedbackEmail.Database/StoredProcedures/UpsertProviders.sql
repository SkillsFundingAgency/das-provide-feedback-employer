CREATE PROCEDURE [dbo].[UpsertProviders]
	@Ukprn BIGINT, @ProviderName VARCHAR(150)
AS
	MERGE [dbo].[Providers] AS [Target]
	USING (SELECT @Ukprn AS Ukprn) AS [Source]
	ON [Target].Ukprn = [Source].Ukprn
	WHEN MATCHED THEN UPDATE SET [Target].ProviderName = @ProviderName, [Target].IsActive = 1
	WHEN NOT MATCHED THEN INSERT (Ukprn, ProviderName, IsActive)
	VALUES(@Ukprn,@ProviderName, 1);