CREATE TABLE [dbo].[EmployerSurveyCodes]
(
	[UniqueSurveyCode] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [UserRef] UNIQUEIDENTIFIER NOT NULL, 
    [Ukprn] INT NOT NULL, 
    [BurnDate] DATETIME NULL 
)
