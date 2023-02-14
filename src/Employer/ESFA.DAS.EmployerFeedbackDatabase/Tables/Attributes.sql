CREATE TABLE [dbo].[Attributes]
(
	[AttributeId] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[AttributeName] VARCHAR(250) NOT NULL
)

GO

CREATE INDEX [IX_Attributes_AttributeId_AttributeName] ON [dbo].[Attributes] ([AttributeId],[AttributeName])