CREATE PROCEDURE [dbo].[GenerateProviderAttributeResults]
(
    @AllUserFeedback INT = 1,        -- Set to 1 (true) to get all user's feedback for each Training provider, or 0 (false) to limit to latest 
    @ResultsforAllTime INT = 0,      -- Set to 1 (true) to get all feedback for each Training provider, or 0 (false) to limit to time period @recentFeedbackMonths
    @recentFeedbackMonths INT = 12  -- Set to limit the time in months to look back for data (default 12, but only actioned if @ResultsforAllTime = 0
)
AS
BEGIN
    DECLARE @CurrentYear INT = YEAR(GETDATE());
    DECLARE @TimePeriods TABLE (TimePeriod VARCHAR(10));
    INSERT INTO @TimePeriods (TimePeriod) VALUES ('All');
    DECLARE @i INT = 0;
    WHILE @i <= 5
    BEGIN
        DECLARE @StartYear INT = @CurrentYear - @i - 1;
        DECLARE @EndYear INT = @CurrentYear - @i;
        DECLARE @TimePeriodTemp VARCHAR(10) = CONCAT('AY', RIGHT(CAST(@StartYear AS VARCHAR), 2), RIGHT(CAST(@EndYear AS VARCHAR), 2));
        INSERT INTO @TimePeriods (TimePeriod) VALUES (@TimePeriodTemp);
        SET @i = @i + 1;
    END
    DECLARE @TimePeriod VARCHAR(10);
    DECLARE @StartDate DATETIME, @EndDate DATETIME;
    DECLARE TimePeriodCursor CURSOR FOR SELECT TimePeriod FROM @TimePeriods;
    OPEN TimePeriodCursor;
    FETCH NEXT FROM TimePeriodCursor INTO @TimePeriod;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @TimePeriod = 'All' AND @ResultsforAllTime = 1
        BEGIN
            SET @StartDate = '1900-01-01';  
            SET @EndDate = GETUTCDATE();
        END
        ELSE
        BEGIN
            IF @TimePeriod = 'All'
            BEGIN
                SET @StartDate = DATEADD(MONTH, -@recentFeedbackMonths, GETUTCDATE());
                SET @EndDate = GETUTCDATE();
            END
            ELSE
            BEGIN
                SET @StartYear = CAST('20' + SUBSTRING(@TimePeriod, 3, 2) AS INT);
                SET @EndYear = CAST('20' + SUBSTRING(@TimePeriod, 5, 2) AS INT);
                SET @StartDate = DATEFROMPARTS(@StartYear, 8, 1);
                SET @EndDate = DATEFROMPARTS(@EndYear, 7, 31);
            END
        END
        ;WITH LatestResults 
    AS (
    SELECT er1.FeedbackId , pa1.AttributeId, pa1.AttributeValue, eft.Ukprn
       FROM (
            SELECT * FROM (
                SELECT ROW_NUMBER() OVER (PARTITION BY FeedbackId ORDER BY DateTimeCompleted DESC) seq, * FROM [dbo].[EmployerFeedbackResult]
                WHERE (DatetimeCompleted BETWEEN @StartDate AND @EndDate)
            ) ab1 WHERE (seq = 1 OR @AllUserFeedback = 1)
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
        FETCH NEXT FROM TimePeriodCursor INTO @TimePeriod;
    END
    CLOSE TimePeriodCursor;
    DEALLOCATE TimePeriodCursor;
END