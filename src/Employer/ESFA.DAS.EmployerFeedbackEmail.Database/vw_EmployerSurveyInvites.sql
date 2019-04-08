CREATE VIEW [dbo].[vw_EmployerSurveyInvites]
	AS 
WITH subquery AS(
        SELECT 
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
    )

SELECT v1.* 
FROM subquery v1
LEFT OUTER JOIN (
     SELECT AccountId, Ukprn, UserRef, Max(InviteSentDate) as 'InviteSentDate', Max(LastReminderSentDate) as 'LastReminderSentDate'
      FROM subquery
      GROUP BY AccountId, Ukprn, UserRef
  ) v2 on v1.AccountId = v2.AccountId AND v1.Ukprn = v2.Ukprn AND v1.UserRef = v2.userRef and v1.InviteSentDate = v2.InviteSentDate