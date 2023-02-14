CREATE PROCEDURE [dbo].[CreateEmployerFeedbackResult]
	@FeedbackId BIGINT, 
	@ProviderRating VARCHAR(20),
	@DateTimeCompleted DateTime2,
	@ProviderAttributesDt ProviderAttributesTemplate READONLY,
	@FeedbackSource INT = 1
AS
	DECLARE @EmployerFeedbackResultId UNIQUEIDENTIFIER;
	DECLARE @EmployerFeedbackResultTableVar TABLE(EmployerFeedbackResultId UNIQUEIDENTIFIER);

	INSERT INTO EmployerFeedbackResult (FeedbackId, ProviderRating, DateTimeCompleted, FeedbackSource)
	OUTPUT inserted.Id
	INTO @EmployerFeedbackResultTableVar
	VALUES (@FeedbackId, @ProviderRating, @DateTimeCompleted, @FeedbackSource)
	
	SELECT @EmployerFeedbackResultId = EmployerFeedbackResultId FROM @EmployerFeedbackResultTableVar
		
	CREATE TABLE #TempProviderAttributes
	(
		EmployerFeedbackResultId UNIQUEIDENTIFIER, 
		AttributeId BIGINT,
		AttributeValue INT
	)

	INSERT INTO #TempProviderAttributes
	SELECT @EmployerFeedbackResultId,* FROM @ProviderAttributesDt

	MERGE [dbo].[ProviderAttributes] as Target
	USING #TempProviderAttributes as Source
	ON Target.EmployerFeedbackResultId = Source.EmployerFeedbackResultId AND Target.AttributeId = Source.AttributeId
	WHEN MATCHED THEN UPDATE
	SET Target.AttributeValue = Source.AttributeValue
	WHEN NOT MATCHED THEN INSERT (EmployerFeedbackResultId, AttributeId, AttributeValue)
	VALUES(Source.EmployerFeedbackResultId, Source.AttributeId, Source.AttributeValue);
	
	SELECT @EmployerFeedbackResultId
	