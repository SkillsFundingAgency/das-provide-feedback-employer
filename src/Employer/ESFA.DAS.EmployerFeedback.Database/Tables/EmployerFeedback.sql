CREATE TABLE [dbo].[EmployerFeedback]
(
	[FeedbackId] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[UserRef] UNIQUEIDENTIFIER NOT NULL,
	[Ukprn] BIGINT NOT NULL,
	[AccountId] BIGINT NOT NULL,
	[IsActive] BIT NOT NULL DEFAULT 0,
	CONSTRAINT FK_ProviderUkprn FOREIGN KEY (Ukprn) REFERENCES [dbo].[Providers](Ukprn),
	CONSTRAINT FK_UserUserRef FOREIGN KEY (UserRef) REFERENCES [dbo].[Users](UserRef)
)

GO

CREATE INDEX [IX_EmployerFeedback_AccountId_Ukprn_UserRef_IsActive] ON [dbo].[EmployerFeedback] ([AccountId], [Ukprn], [UserRef], [IsActive]) WITH (ONLINE = ON)
