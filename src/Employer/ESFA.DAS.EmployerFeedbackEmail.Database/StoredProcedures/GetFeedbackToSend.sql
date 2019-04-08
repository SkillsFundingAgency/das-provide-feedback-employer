CREATE PROCEDURE [dbo].[GetFeedbackToSend]
AS
	SELECT 
	U.EmailAddress,
	U.FirstName,
	P.ProviderName,
	U.AccountId,
	P.Ukprn,
	U.UserRef
	FROM [dbo].[Feedback] F
	JOIN [dbo].[Users] U
	ON F.UserRef = U.UserRef
	JOIN [dbo].[Providers] P
	ON F.Ukprn = P.Ukprn
	WHERE F.IsActive = 1