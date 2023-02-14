CREATE TABLE #TempAttributes
(
	[AttributeId] BIGINT,
	[AttributeName] VARCHAR(250)
)
 
INSERT INTO #TempAttributes (AttributeId, AttributeName)
VALUES
(1,'Providing the right training at the right time'),
(2,'Communication with employers'),
(3,'Getting new apprentices started'),
(4,'Improving apprentice skills'),
(5,'Working with small numbers of apprentices'),
(8,'Adapting to my needs'),
(6,'Initial assessment of apprentices'),
(7,'Reporting on progress of apprentices'),
(9,'Training facilities')

SET IDENTITY_INSERT [dbo].[Attributes] ON;

MERGE [dbo].[Attributes] AS Target
USING #TempAttributes AS Source
ON Source.AttributeId = Target.AttributeId
WHEN MATCHED THEN UPDATE SET
	Target.AttributeName = Source.AttributeName

WHEN NOT MATCHED By Target THEN
	INSERT (AttributeId, AttributeName) 
	VALUES (Source.AttributeId, Source.AttributeName);

SET IDENTITY_INSERT [dbo].[Attributes] OFF;
DROP TABLE #TempAttributes