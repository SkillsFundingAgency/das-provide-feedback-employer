CREATE TABLE [dbo].[EmployerSurveyCodes]
(
	[UniqueSurveyCode] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [FeedbackId] BIGINT NOT NULL,
    [BurnDate] DATETIME NULL,
	CONSTRAINT FK_EmployerFeedbackFeedbackId FOREIGN KEY (FeedbackId) REFERENCES [dbo].[EmployerFeedback](FeedbackId),
)
