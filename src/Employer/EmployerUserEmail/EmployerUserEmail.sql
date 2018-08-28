CREATE TABLE [EmployerUser].[cs]
(
	[EmailUID] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [EmailCode] UNIQUEIDENTIFIER NOT NULL, 
    [EmailAddress] NVARCHAR(100) NOT NULL, 
    [UserRef] UNIQUEIDENTIFIER NOT NULL, 
    [AccountId] INT NOT NULL, 
    [ProviderId] INT NOT NULL, 
    [EmailSentDate] DATETIME NULL, 
    [CodeBurntDate] DATETIME NULL
)
