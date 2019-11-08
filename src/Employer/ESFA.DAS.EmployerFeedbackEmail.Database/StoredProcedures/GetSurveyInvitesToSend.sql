CREATE PROCEDURE [dbo].[GetSurveyInvitesToSend]
AS
	SELECT * FROM vw_EmployerSurveyInvites
	WHERE InviteSentDate IS NULL
RETURN 0
