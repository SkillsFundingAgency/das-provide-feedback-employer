CREATE PROCEDURE [dbo].[CreateEmployerEmailDetails_Beta]
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
	FROM staging_employer_accounts ea
	INNER JOIN staging_commitment c
	ON ea.AccountId = c.EmployerAccountId
	INNER JOIN staging_roatp r
	ON c.ProviderId = r.Ukprn
	WHERE r.ProviderType = 'Main Provider'
	AND ea.EmailAddress IN (SELECT EmailAddress FROM staging_beta_users)
RETURN 0
