CREATE PROCEDURE [dbo].[GetSurveyRemindersToSend]
	@MinSentDate DATETIME
AS
	SELECT * FROM vw_EmployerSurveyInvites
    WHERE InviteSentDate IS NOT NULL
    AND LastReminderSentDate IS NULL
    AND InviteSentDate < @MinSentDate
    AND CodeBurntDate IS NULL
RETURN 0
