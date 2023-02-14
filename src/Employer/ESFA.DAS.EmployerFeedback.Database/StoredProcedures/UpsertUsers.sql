CREATE PROCEDURE [dbo].[UpsertUsers]
	@UserRef UNIQUEIDENTIFIER,
	@EmailAddress NVarchar(50),
	@FirstName NVarchar(150)
AS
BEGIN
	  UPDATE [dbo].[Users] 
			 SET [EmailAddress] = @EmailAddress,	[FirstName] = @FirstName
			 WHERE [dbo].[Users].UserRef = @UserRef;
	  
	  IF @@ROWCOUNT = 0		
		BEGIN		
			INSERT INTO [dbo].[Users]
				([UserRef],[EmailAddress],[FirstName])
			VALUES
				(@UserRef,@EmailAddress,@FirstName)
		END
END