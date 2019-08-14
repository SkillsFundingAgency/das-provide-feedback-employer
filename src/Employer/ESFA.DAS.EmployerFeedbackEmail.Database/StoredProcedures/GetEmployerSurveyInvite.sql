CREATE PROCEDURE [dbo].[GetEmployerSurveyInvite]
	@feedbackId BIGINT
AS
	SELECT TOP(1) esi.* 
	FROM vw_EmployerSurveyInvites esi
	INNER JOIN vw_FeedbackToSend fb
	ON esi.AccountId = fb.AccountId AND esi.Ukprn = fb.Ukprn AND esi.UserRef = fb.UserRef
	WHERE fb.FeedbackId = @feedbackId
RETURN 0
