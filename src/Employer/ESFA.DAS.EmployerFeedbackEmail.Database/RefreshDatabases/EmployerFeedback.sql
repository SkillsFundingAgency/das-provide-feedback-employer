CREATE TABLE [dbo].[EmployerFeedback]
(
	[UserRef] UNIQUEIDENTIFIER NOT NULL,
	[Ukprn] BIGINT NOT NULL,
	[AccountId] BIGINT NOT NULL,
	[IsActive] BIT NOT NULL DEFAULT 0,
	CONSTRAINT FK_ProviderUkprn FOREIGN KEY (Ukprn) REFERENCES [dbo].[Providers](Ukprn),
	CONSTRAINT FK_UserUserRef FOREIGN KEY (UserRef) REFERENCES [dbo].[Users](UserRef)
)
