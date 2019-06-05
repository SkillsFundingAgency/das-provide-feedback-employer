CREATE TABLE [dbo].[EmployerEmailDetails]
(
	[EmailUID] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [EmailCode] UNIQUEIDENTIFIER NOT NULL, 
    [EmailAddress] NVARCHAR(100) NOT NULL, 
    [UserRef] UNIQUEIDENTIFIER NOT NULL, 
    [UserFirstName] NVARCHAR(100) NOT NULL,
	[AccountId] INT NOT NULL, 
    [Ukprn] INT NOT NULL,
	[ProviderName] NVARCHAR(100) NOT NULL,
    [EmailSentDate] DATETIME NULL, 
    [CodeBurntDate] DATETIME NULL,
	[EmailReminderSentDate] DATETIME NULL
    
)


GO

CREATE NONCLUSTERED INDEX [IX_EmployerEmailDetails_EmailCode] ON [dbo].[EmployerEmailDetails] ([EmailCode]) WITH (ONLINE = ON)
GO
CREATE NONCLUSTERED INDEX [IX_EmployerEmailDetails_UserRef] ON [dbo].[EmployerEmailDetails] (UserRef) WITH (ONLINE = ON)
