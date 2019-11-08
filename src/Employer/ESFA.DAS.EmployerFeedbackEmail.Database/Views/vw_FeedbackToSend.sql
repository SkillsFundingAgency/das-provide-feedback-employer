CREATE VIEW [dbo].[vw_FeedbackToSend]
AS
	SELECT 
	F.FeedbackId,
	U.EmailAddress,
	U.FirstName,
	P.ProviderName,
	F.AccountId,
	P.Ukprn,
	U.UserRef,
	F.IsActive
	FROM [dbo].[EmployerFeedback] F
	JOIN [dbo].[Users] U
	ON F.UserRef = U.UserRef
	JOIN [dbo].[Providers] P
	ON F.Ukprn = P.Ukprn