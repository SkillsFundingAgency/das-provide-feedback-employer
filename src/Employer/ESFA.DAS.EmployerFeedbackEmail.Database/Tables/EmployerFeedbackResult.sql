CREATE TABLE [dbo].[EmployerFeedbackResult]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY NONCLUSTERED DEFAULT NEWSEQUENTIALID(),
	[FeedbackId] BIGINT NOT NULL,
	[DateTimeCompleted] DATETIME2 NOT NULL,
	[ProviderRating] VARCHAR(20) NOT NULL,
	[FeedbackSource] INT NULL DEFAULT 1,
	CONSTRAINT FK_EmployerFeedbackResult_EmployerFeedback_FeedbackId FOREIGN KEY (FeedbackId) REFERENCES [dbo].[EmployerFeedback](FeedbackId)
)

GO

CREATE INDEX [IX_EmployerFeedbackResult] ON [dbo].[EmployerFeedbackResult] ([Id], [FeedbackId]) INCLUDE ([DateTimeCompleted], [ProviderRating])
GO
CREATE UNIQUE INDEX [IX_EmployerFeedbackResult_UniqueResult] ON [dbo].[EmployerFeedbackResult] ([FeedbackId],[DateTimeCompleted])
