CREATE TABLE [dbo].[ProviderAttributes]
(
	[EmployerFeedbackResultId] UNIQUEIDENTIFIER NOT NULL,
	[AttributeId] BIGINT NOT NULL,
	[AttributeValue] INT NOT NULL
	CONSTRAINT PK_ProviderAttributes_EmployerFeedbackResultId_AttributeId PRIMARY KEY NONCLUSTERED (EmployerFeedbackResultId,AttributeId)
	CONSTRAINT FK_ProviderAttributes_EmployerFeedbackResult_EmployerFeedbackResultId FOREIGN KEY (EmployerFeedbackResultId) REFERENCES [dbo].[EmployerFeedbackResult](Id)
	CONSTRAINT FK_ProviderAttributes_Attributes_AttributeId FOREIGN KEY (AttributeId) REFERENCES [dbo].[Attributes](AttributeId)
)

GO