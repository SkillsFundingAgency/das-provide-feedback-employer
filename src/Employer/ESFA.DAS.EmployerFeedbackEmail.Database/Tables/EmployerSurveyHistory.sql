CREATE TABLE [dbo].[EmployerSurveyHistory]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY, 
	[UniqueSurveyCode] UNIQUEIDENTIFIER NOT NULL,
    [EmailType] INT NOT NULL, 
    [SentDate] DATETIME NOT NULL, 
    CONSTRAINT [FK_EmployerSurveyHistory_EmployerSurveyCodes] FOREIGN KEY ([UniqueSurveyCode]) REFERENCES [EmployerSurveyCodes]([UniqueSurveyCode])
)

GO

CREATE INDEX [IX_EmployerSurveyHistory_UniqueSurveyCode] ON [dbo].[EmployerSurveyHistory] ([UniqueSurveyCode])
