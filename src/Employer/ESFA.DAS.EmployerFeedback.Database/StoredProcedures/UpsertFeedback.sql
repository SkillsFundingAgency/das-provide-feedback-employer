CREATE PROCEDURE [dbo].[UpsertFeedback]
	@UserRef UNIQUEIDENTIFIER, @Ukprn BIGINT, @AccountId BIGINT
AS
	DECLARE @FeedbackId BIGINT;

	MERGE [dbo].[EmployerFeedback] AS [Target]
	USING (SELECT @UserRef AS UserRef, @Ukprn AS Ukprn,@AccountId AS AccountId) AS [Source]
	ON [Target].UserRef = [Source].UserRef AND [Target].Ukprn = [Source].Ukprn AND [Target].AccountId = [Source].AccountId
	WHEN MATCHED THEN UPDATE 
	SET [Target].IsActive = 1, @FeedbackId = [Target].FeedbackId
	WHEN NOT MATCHED THEN INSERT (UserRef, Ukprn, AccountId, IsActive)
	VALUES(@UserRef,@Ukprn,@AccountId,1);
	
	SELECT CASE WHEN @FeedbackId IS NULL THEN  SCOPE_IDENTITY() ELSE @FeedbackId END;
	