CREATE VIEW [dbo].[vw_EmployerSurveyInvites]
	AS SELECT 
	esc.UniqueSurveyCode,
	u.UserRef,
	u.EmailAddress,
	u.FirstName,
	acc.AccountId,
	prov.Ukprn,
	prov.ProviderName,
	inviteHistory.SentDate as 'InviteSentDate',
	reminderHistory.SentDate as 'LastReminderSentDate',
	esc.BurnDate as 'CodeBurntDate'
	FROM EmployerSurveyCodes esc
	RIGHT JOIN Users u on u.UserRef = esc.UserRef
	RIGHT JOIN Accounts acc on acc.AccountId = u.AccountId
	JOIN Providers prov on prov.Ukprn = esc.Ukprn
	LEFT JOIN (SELECT h.UniqueSurveyCode, h.SentDate FROM EmployerSurveyHistory h WHERE h.EmailType = 1) as inviteHistory on inviteHistory.UniqueSurveyCode = esc.UniqueSurveyCode
	LEFT JOIN (SELECT h.UniqueSurveyCode, h.SentDate FROM EmployerSurveyHistory h WHERE h.EmailType = 2) as reminderHistory on reminderHistory.UniqueSurveyCode = esc.UniqueSurveyCode
	WHERE acc.IsActive = 1
	AND u.IsActive = 1
