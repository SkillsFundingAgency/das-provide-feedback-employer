CREATE TABLE [dbo].[Users]
(
	[UserRef] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [AccountId] INT NOT NULL, 
    [EmailAddress] NCHAR(100) NOT NULL, 
    [FirstName] NCHAR(50) NOT NULL, 
    [IsActive] BIT NOT NULL, 
    CONSTRAINT [FK_Users_Accounts] FOREIGN KEY ([AccountId]) REFERENCES [Accounts]([AccountId])
)
