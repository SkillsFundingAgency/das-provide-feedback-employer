CREATE TYPE [dbo].[UserTemplate] AS TABLE
(
	UserRef UniqueIdentifier, 
	FirstName nvarchar(50),
	EmailAddress nvarchar(150)
)
