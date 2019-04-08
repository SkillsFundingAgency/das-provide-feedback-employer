CREATE PROCEDURE [dbo].[UpsertProviders]
	@Ukprn BIGINT, @ProviderName VARCHAR(150)
AS
	MERGE [dbo].[Providers] AS [Target]
	USING (SELECT @Ukprn AS Ukprn) AS [Source]
	ON [Target].Ukprn = [Source].Ukprn
	WHEN MATCHED THEN UPDATE SET [Target].ProviderName = @ProviderName
	WHEN NOT MATCHED THEN INSERT (Ukprn, ProviderName)
	VALUES(@Ukprn,@ProviderName);