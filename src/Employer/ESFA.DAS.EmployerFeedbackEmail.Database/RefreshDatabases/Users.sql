CREATE TABLE [dbo].[Users]
(
	[UserRef] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [AccountId] BIGINT NOT NULL, 
    [EmailAddress] NCHAR(150) NOT NULL, 
    [FirstName] NCHAR(50) NOT NULL, 
)
