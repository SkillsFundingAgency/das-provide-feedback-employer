CREATE PROCEDURE [dbo].[UpsertFeedback]
	@UserRef UNIQUEIDENTIFIER, @Ukprn BIGINT, @AccountId BIGINT
AS
	MERGE [dbo].[EmployerFeedback] AS [Target]
	USING (SELECT(SELECT @UserRef) AS UserRef,(SELECT @Ukprn)AS Ukprn,(SELECT @AccountId)AS AccountId) AS [Source]
	ON [Target].UserRef = [Source].UserRef AND [Target].Ukprn = [Source].Ukprn AND [Target].AccountId = [Source].AccountId
	WHEN MATCHED THEN UPDATE 
	SET [Target].IsActive = 1
	WHEN NOT MATCHED THEN INSERT (UserRef, Ukprn, AccountId, IsActive)
	VALUES(@UserRef,@Ukprn,@AccountId,1);