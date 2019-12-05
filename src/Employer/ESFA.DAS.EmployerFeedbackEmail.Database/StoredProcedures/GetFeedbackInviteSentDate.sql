CREATE PROCEDURE [dbo].[GetFeedbackInviteSentDate]
	@FeedbackId int
AS
	DECLARE @inviteEmailType INT = 1;
	SELECT 
		EFB.FeedbackId, ESC.UniqueSurveyCode, SentDate AS InviteSentDate
	FROM 
		dbo.EmployerFeedback EFB
		LEFT JOIN EmployerSurveyCodes ESC ON EFB.FeedbackId = ESC.FeedbackId
		LEFT JOIN EmployerSurveyHistory ESH ON ESH.UniqueSurveyCode = ESC.UniqueSurveyCode
	WHERE 
		( ESH.EmailType IS NULL OR ESH.EmailType = @inviteEmailType )
		AND EFB.FeedbackId = @FeedbackId
