CREATE PROCEDURE [dbo].[CreateSurveyCode]
	@UserRef UNIQUEIDENTIFIER, @Ukprn BIGINT, @AccountId BIGINT
AS

	DECLARE @feedbackId BIGINT;
	SELECT @feedbackId = feedbackId FROM EmployerFeedback WHERE UserRef = @UserRef AND Ukprn = @Ukprn AND AccountId = @AccountId
	
	MERGE [dbo].[EmployerSurveyCodes] AS [Target]
	USING (SELECT(SELECT @FeedbackId) AS FeedbackId) AS [Source]
	ON [Target].FeedbackId = [Source].FeedbackId AND [Target].BurnDate IS NULL
	WHEN NOT MATCHED THEN INSERT (UniqueSurveyCode, FeedbackId, BurnDate)
	VALUES(NewId(), @feedbackId, NULL);
GO