CREATE PROCEDURE [dbo].[GetEmployerSurveyHistory]
	@feedbackId BIGINT
AS
	SELECT TOP(1) his.* 
	FROM vw_EmployerSurveyHistoryComplete his
	INNER JOIN vw_FeedbackToSend fb
	ON his.AccountId = fb.AccountId AND his.Ukprn = fb.Ukprn AND his.UserRef = fb.UserRef
	WHERE fb.FeedbackId = @feedbackId
RETURN 0
