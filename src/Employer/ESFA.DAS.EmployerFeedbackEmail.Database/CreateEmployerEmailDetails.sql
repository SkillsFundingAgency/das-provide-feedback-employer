CREATE PROCEDURE [dbo].[CreateEmployerEmailDetails]
AS
	INSERT INTO EmployerEmailDetails
	SELECT 
	 NEWID() AS 'EmailUID'
	,NEWID() AS 'EmailCode'
	,ea.EmailAddress AS 'EmailAddress'
	,ea.UserRef
	,ea.FirstName
	,ea.AccountId
	,c.ProviderId
	,r.Name
	,NULL AS 'EmailSentDate'
	,NULL AS 'CodeBurntDate'
	,NULL AS 'ReminderEmailSentDate'
	FROM staging_employer_accounts ea
	INNER JOIN staging_commitment c
	ON ea.AccountId = c.EmployerAccountId
	INNER JOIN staging_roatp r
	ON c.ProviderId = r.Ukprn
	WHERE r.ProviderType = 'Main Provider'
	AND ea.EmailAddress NOT IN (SELECT EmailAddress FROM EmployerEmailDetails)
RETURN 0