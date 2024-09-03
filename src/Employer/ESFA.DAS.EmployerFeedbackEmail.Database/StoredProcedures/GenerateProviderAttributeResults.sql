CREATE PROCEDURE [dbo].[GenerateProviderAttributeResults]
(
    @AllUserFeedback INT = 1,        -- Set to 1 (true) to get all user's feedback for each Training provider, or 0 (false) to limit to latest :   The parameter is not being used. It should be removed
    @ResultsforAllTime INT = 0,      -- Set to 1 (true) to get all feedback for each Training provider, or 0 (false) to limit to time period @recentFeedbackMonths :   The parameter is not being used. It should be removed
    @recentFeedbackMonths INT = 12  -- Set to limit the time in months to look back for data (default 12, but only actioned if @ResultsforAllTime = 0 :   The parameter is not being used. It should be removed
)
AS
BEGIN
DECLARE @CurrentDate DATE = GETDATE();
DECLARE @CurrentYear INT = YEAR(@CurrentDate);
DECLARE @StartYear INT = YEAR(DATEADD(YEAR, -5, @CurrentDate));
DECLARE @EndYear INT = YEAR(@CurrentDate);
DECLARE @TimePeriods TABLE (ID INT IDENTITY(1,1), TimePeriod VARCHAR(10), StartDate DATETIME, EndDate DATETIME);
DECLARE @AcademicStartYear INT;
DECLARE @AcademicEndYear INT;
DECLARE @TimePeriodTemp VARCHAR(10);

IF @CurrentDate <= DATEFROMPARTS(@CurrentYear, 7, 31)
BEGIN
    SET @EndYear = @CurrentYear;
    SET @StartYear = @EndYear - 5;
END
ELSE
BEGIN
    SET @EndYear = @CurrentYear + 1;
    SET @StartYear = @EndYear - 5;
END

WHILE @StartYear < @EndYear
BEGIN

    SET @AcademicStartYear = @StartYear;
    SET @AcademicEndYear = @StartYear + 1;

	    SET @TimePeriodTemp = CONCAT('AY', RIGHT(CAST(@AcademicStartYear AS VARCHAR), 2), RIGHT(CAST(@AcademicEndYear AS VARCHAR), 2));
        IF NOT EXISTS (SELECT 1 FROM @TimePeriods WHERE TimePeriod = @TimePeriodTemp)
        BEGIN
            INSERT INTO @TimePeriods (TimePeriod, StartDate, EndDate) 
	        VALUES (@TimePeriodTemp,DATETIMEFROMPARTS(@AcademicStartYear, 8, 1, 0, 0, 0, 0), DATETIMEFROMPARTS(@AcademicEndYear, 7, 31, 23, 59, 59, 997));
        END

     SET @StartYear += 1;
END

DECLARE @TimePeriod VARCHAR(10);
DECLARE @StartDate DATETIME;
DECLARE @EndDate DATETIME;
DECLARE @RowNum INT = 1;
DECLARE @TotalRows INT = (SELECT COUNT(*) FROM @TimePeriods);

DELETE FROM [dbo].[ProviderAttributeSummary]
WHERE TimePeriod NOT IN (SELECT TimePeriod FROM @TimePeriods) AND TimePeriod != 'All';

WHILE @RowNum <= @TotalRows
BEGIN
    SELECT @TimePeriod = TimePeriod, @StartDate = StartDate, @EndDate = EndDate
    FROM @TimePeriods
    WHERE ID = @RowNum;
  
	;WITH LatestResults 
    AS (
    SELECT er1.FeedbackId , pa1.AttributeId, pa1.AttributeValue, eft.Ukprn
       FROM (
            SELECT * FROM (
                SELECT ROW_NUMBER() OVER (PARTITION BY FeedbackId ORDER BY DateTimeCompleted DESC) seq, * FROM [dbo].[EmployerFeedbackResult]
                WHERE (DatetimeCompleted BETWEEN @StartDate AND @EndDate)
            ) ab1 WHERE (YEAR(@EndDate) < 2025) OR (YEAR(@EndDate) >= 2025 AND seq = 1)
        ) er1
        JOIN [dbo].[EmployerFeedback] eft on er1.FeedbackId = eft.FeedbackId
        JOIN [dbo].[ProviderAttributes] pa1 on pa1.EmployerFeedbackResultId = er1.Id
    WHERE (DatetimeCompleted BETWEEN @StartDate AND @EndDate) 
    )
    MERGE INTO [dbo].[ProviderAttributeSummary] pas 
    USING (      
    SELECT Ukprn, AttributeId
    , SUM(CASE WHEN AttributeValue = 1 THEN 1 ELSE 0 END) Strength 
    , SUM(CASE WHEN AttributeValue = 1 THEN 0 ELSE 1 END) Weakness 
    , GETUTCDATE() UpdatedOn
    FROM LatestResults
    GROUP BY Ukprn, AttributeId
    ) upd
    ON pas.Ukprn = upd.Ukprn AND pas.AttributeId = upd.AttributeId AND pas.TimePeriod = @TimePeriod
    WHEN MATCHED THEN 
        UPDATE SET pas.Strength = upd.Strength, 
                   pas.Weakness = upd.Weakness,
                   pas.UpdatedOn = upd.UpdatedOn
    WHEN NOT MATCHED BY TARGET THEN 
        INSERT (Ukprn, AttributeId, Strength, Weakness, UpdatedOn,TimePeriod) 
        VALUES (upd.Ukprn, upd.AttributeId, upd.Strength, upd.Weakness, upd.UpdatedOn,@TimePeriod)
        WHEN NOT MATCHED BY SOURCE AND pas.TimePeriod = @TimePeriod THEN
        DELETE;
    SET @RowNum += 1;
END
BEGIN
    ;WITH AllResults AS (
        SELECT Ukprn, AttributeId, 
               SUM(Strength) AS Agree, 
               SUM(Weakness) AS Disagree
        FROM [dbo].[ProviderAttributeSummary]
        WHERE TimePeriod <> 'All'
        GROUP BY Ukprn, AttributeId
    )
    MERGE INTO [dbo].[ProviderAttributeSummary] pas
    USING (
        SELECT Ukprn, AttributeId, Agree, Disagree, GETUTCDATE() AS UpdatedOn
        FROM AllResults
    ) upd
    ON pas.Ukprn = upd.Ukprn AND pas.AttributeId = upd.AttributeId AND pas.TimePeriod = 'All'
    WHEN MATCHED THEN
        UPDATE SET pas.Strength = upd.Agree, 
                   pas.Weakness = upd.Disagree,
                   pas.UpdatedOn = upd.UpdatedOn
    WHEN NOT MATCHED BY TARGET THEN
		INSERT (Ukprn, AttributeId, Strength, Weakness, UpdatedOn,TimePeriod) 
        VALUES (upd.Ukprn, upd.AttributeId, upd.Agree, upd.Disagree, upd.UpdatedOn,'All')
	WHEN NOT MATCHED BY SOURCE AND pas.TimePeriod = 'All' THEN
        DELETE;
END
END
