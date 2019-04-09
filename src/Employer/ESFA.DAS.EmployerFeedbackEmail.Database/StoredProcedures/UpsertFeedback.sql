CREATE PROCEDURE [dbo].[UpsertFeedback]
	@UserRef UNIQUEIDENTIFIER, @Ukprn BIGINT
AS
	MERGE [dbo].[EmployerFeedback] AS [Target]
	USING (SELECT(SELECT @UserRef) AS UserRef,(SELECT @Ukprn)AS Ukprn) AS [Source]
	ON [Target].UserRef = [Source].UserRef AND [Target].Ukprn = [Source].Ukprn
	WHEN MATCHED THEN UPDATE 
	SET [Target].IsActive = 1
	WHEN NOT MATCHED THEN INSERT (UserRef, Ukprn, IsActive)
	VALUES(@UserRef,@Ukprn,1);