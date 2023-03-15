CREATE TABLE [dbo].[staging_employer_accounts]
(
	[AccountId] INT NOT NULL, 
    [UserRef] UNIQUEIDENTIFIER NOT NULL, 
    [EmailAddress] NCHAR(200) NOT NULL, 
    [FirstName] NCHAR(75) NOT NULL 
)
