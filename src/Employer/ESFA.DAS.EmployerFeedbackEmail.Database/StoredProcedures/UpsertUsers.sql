CREATE PROCEDURE [dbo].[UpsertUsers]
	@UserRef UNIQUEIDENTIFIER,
	@EmailAddress NVarchar(50),
	@FirstName NVarchar(150)
AS
BEGIN
	IF EXISTS (Select * from dbo.Users WHERE UserRef = @UserRef)
		BEGIN
			UPDATE [dbo].[Users] 
			SET [EmailAddress] = @EmailAddress,	[FirstName] = @FirstName
			WHERE [dbo].[Users].UserRef = @UserRef;
		END
	ELSE
		BEGIN
			INSERT INTO [dbo].[Users]
           ([UserRef],[EmailAddress],[FirstName])
        VALUES
           (@UserRef,@EmailAddress,@FirstName)
		END
END