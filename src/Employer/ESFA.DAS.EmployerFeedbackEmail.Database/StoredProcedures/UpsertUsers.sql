CREATE PROCEDURE [dbo].[UpsertUsers]
	@UserRef UNIQUEIDENTIFIER, @FirstName VARCHAR(100), @EmailAddress VARCHAR(150)
AS
	MERGE [dbo].[Users] AS [Target]
	USING (SELECT @UserRef AS UserRef) AS [Source]
	ON [Target].UserRef = [Source].UserRef
	WHEN MATCHED THEN UPDATE 
	SET [Target].FirstName = @FirstName,
	[Target].EmailAddress = @EmailAddress
	WHEN NOT MATCHED THEN INSERT (UserRef, FirstName,EmailAddress)
	VALUES(@UserRef, @FirstName,@EmailAddress);