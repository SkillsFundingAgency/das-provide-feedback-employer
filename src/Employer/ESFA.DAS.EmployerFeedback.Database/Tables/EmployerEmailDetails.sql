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