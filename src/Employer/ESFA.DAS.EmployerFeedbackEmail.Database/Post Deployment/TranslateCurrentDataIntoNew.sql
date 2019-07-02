EXEC UpsertUsers (SELECT UserRef, UserFirstName, EmailAddress FROM EmployerEmailDetails)

EXEC UpsertProviders (SELECT Ukprn, ProviderName FROM EmployerEmailDetails)

EXEC UpsertFeedback (SELECT UserRef, Ukprn, AccountId FROM EmployerEmailDetails)

EXEC ResetFeedback

INSERT INTO EmployerSurveyCodes (UniqueSurveyCode, UserRef, Ukprn, AccountId, BurnDate)
SELECT EmailCode, UserRef, Ukprn, AccountId, CodeBurntDate FROM EmployerEmailDetails

INSERT INTO EmployerSurveyHistory (UniqueSurveyCode, SentDate, EmailType)
SELECT EmailCode, EmailSentDate, 1 FROM EmployerEmailDetails WHERE EmailSentDate IS NOT NULL

INSERT INTO EmployerSurveyHistory (UniqueSurveyCode, SentDate, EmailType)
SELECT EmailCode, EmailReminderSentDate, 2 FROM EmployerEmailDetails WHERE EmailReminderSentDate IS NOT NULL