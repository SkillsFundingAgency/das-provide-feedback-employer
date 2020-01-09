CREATE PROCEDURE [dbo].[GetLatestFeedbackInviteSentDate]
	@FeedbackId int
AS
	DECLARE @inviteEmailType INT = 1;
	-- There could be more than one survey code for each feedback as they get generated every three months
	-- hence this is ordered by history sent date and the latest top record is returned. 
	-- As seen on production, there could be multiple survey codes with no history or history with same sent date
	-- this is unexpected but to handle the situation the data is also sorted by survey code to get a consistent outcome 
	SELECT 
		TOP 1
		EFB.FeedbackId, ESC.UniqueSurveyCode, ESH.SentDate AS InviteSentDate
	FROM 
		dbo.EmployerFeedback EFB
		LEFT JOIN EmployerSurveyCodes ESC ON EFB.FeedbackId = ESC.FeedbackId
		LEFT JOIN EmployerSurveyHistory ESH ON ESH.UniqueSurveyCode = ESC.UniqueSurveyCode
	WHERE 
		( ESH.EmailType IS NULL OR ESH.EmailType = @inviteEmailType )
		AND EFB.FeedbackId = @FeedbackId
	ORDER BY
		ESH.SentDate DESC, ESC.UniqueSurveyCode
