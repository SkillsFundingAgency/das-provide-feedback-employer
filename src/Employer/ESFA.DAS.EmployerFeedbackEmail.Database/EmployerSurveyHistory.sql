CREATE TABLE [dbo].[EmployerSurveyHistory]
(
	[Id] INT NOT NULL PRIMARY KEY, 
	[UniqueSurveyCode] UNIQUEIDENTIFIER NOT NULL,
    [EmailType] INT NOT NULL, 
    [SentDate] DATETIME NOT NULL, 
    CONSTRAINT [FK_EmployerSurveyHistory_EmployerSurveyCodes] FOREIGN KEY ([UniqueSurveyCode]) REFERENCES [EmployerSurveyCodes]([UniqueSurveyCode])
)
