CREATE VIEW [dbo].[vw_GetFeedbackToSend]
AS
	SELECT 
	U.EmailAddress,
	U.FirstName,
	P.ProviderName,
	F.AccountId,
	P.Ukprn,
	U.UserRef
	FROM [dbo].[EmployerFeedback] F
	JOIN [dbo].[Users] U
	ON F.UserRef = U.UserRef
	JOIN [dbo].[Providers] P
	ON F.Ukprn = P.Ukprn
	WHERE F.IsActive = 1