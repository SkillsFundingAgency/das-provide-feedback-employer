﻿CREATE VIEW [dbo].[vw_EmployerSurveyInvites]
	AS 
WITH subquery AS(
        SELECT 
        esc.UniqueSurveyCode,
        FB.UserRef,
        FB.EmailAddress,
        FB.FirstName,
        FB.AccountId,
        FB.Ukprn,
        FB.ProviderName,
        inviteHistory.SentDate as 'InviteSentDate',
        reminderHistory.SentDate as 'LastReminderSentDate',
        esc.BurnDate as 'CodeBurntDate'
        FROM EmployerSurveyCodes esc
		LEFT JOIN (SELECT EmailAddress, FirstName, ProviderName, AccountId, Ukprn, UserRef FROM [dbo].[vw_FeedbackToSend]) AS FB on FB.UserRef = esc.UserRef AND FB.Ukprn = esc.Ukprn AND FB.AccountId = esc.AccountId
        LEFT JOIN (SELECT h.UniqueSurveyCode, h.SentDate FROM EmployerSurveyHistory h WHERE h.EmailType = 1) as inviteHistory on inviteHistory.UniqueSurveyCode = esc.UniqueSurveyCode
        LEFT JOIN (SELECT h.UniqueSurveyCode, h.SentDate FROM EmployerSurveyHistory h WHERE h.EmailType = 2) as reminderHistory on reminderHistory.UniqueSurveyCode = esc.UniqueSurveyCode
    )

SELECT v1.* 
FROM subquery v1
LEFT OUTER JOIN (
     SELECT AccountId, Ukprn, UserRef, Max(InviteSentDate) as 'InviteSentDate', Max(LastReminderSentDate) as 'LastReminderSentDate'
      FROM subquery
      GROUP BY AccountId, Ukprn, UserRef
  ) v2 on v1.AccountId = v2.AccountId AND v1.Ukprn = v2.Ukprn AND v1.UserRef = v2.userRef and v1.InviteSentDate = v2.InviteSentDate