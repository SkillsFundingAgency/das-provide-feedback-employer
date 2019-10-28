CREATE PROCEDURE [dbo].[UpsertUsers]
	@UsersDt UserTemplate READONLY
AS
	MERGE [dbo].[Users] AS [Target]
	USING @UsersDt AS [Source]
	ON [Target].UserRef = [Source].UserRef
	WHEN MATCHED THEN UPDATE 
	SET [Target].FirstName = [Source].FirstName,
	[Target].EmailAddress = [Source].EmailAddress
	WHEN NOT MATCHED THEN INSERT (UserRef, FirstName,EmailAddress)
	VALUES([Source].UserRef, [Source].FirstName,[Source].EmailAddress);