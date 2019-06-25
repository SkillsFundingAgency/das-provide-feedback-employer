CREATE TABLE [dbo].[EmployerSurveyCodes]
(
	[UniqueSurveyCode] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [UserRef] UNIQUEIDENTIFIER NOT NULL, 
    [Ukprn] BIGINT NOT NULL,
	[AccountId] BIGINT NOT NULL,
    [BurnDate] DATETIME NULL 
)
