﻿CREATE TABLE [dbo].[Users]
(
	[UserRef] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
	[EmailAddress] NVARCHAR(150) NOT NULL, 
    [FirstName] NVARCHAR(50) NOT NULL, 
)