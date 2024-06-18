CREATE PROCEDURE [dbo].[GenerateProviderRatingResults]
(
    @AllUserFeedback INT = 1,        -- Set to 1 (true) to get all user's feedback for each Training provider, or 0 (false) to limit to latest 
    @ResultsforAllTime INT = 0,      -- Set to 1 (true) to get all feedback for each Training provider, or 0 (false) to limit to time period @recentFeedbackMonths
    @recentFeedbackMonths INT = 12,  -- Set to limit the time in months to look back for data (default 12, but only actioned if @ResultsforAllTime = 0
    @tolerance FLOAT = 0.3           -- 0.3 is the current Employer Feedback tolerance, set to 0.5 to match Apprentice feedback
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
        ;WITH LatestRatings 
    AS (
    SELECT er1.FeedbackId, er1.ProviderRating, eft.Ukprn
       FROM (
            SELECT * FROM (
                SELECT ROW_NUMBER() OVER (PARTITION BY FeedbackId ORDER BY DateTimeCompleted DESC) seq, * FROM [dbo].[EmployerFeedbackResult]
                WHERE (DatetimeCompleted BETWEEN @StartDate AND @EndDate)
            ) ab1 WHERE (seq = 1 OR @AllUserFeedback = 1)
        ) er1
        JOIN [dbo].[EmployerFeedback] eft on er1.FeedbackId = eft.FeedbackId
    WHERE (DatetimeCompleted BETWEEN @StartDate AND @EndDate)
    )
    MERGE INTO [dbo].[ProviderRatingSummary]  prs
    USING (    
    SELECT Ukprn, ProviderRating Rating, count(*) RatingCount, GETUTCDATE() UpdatedOn
    FROM LatestRatings
    GROUP BY Ukprn, ProviderRating
    ) upd
    ON prs.Ukprn = upd.Ukprn AND prs.Rating = upd.Rating AND prs.TimePeriod = @TimePeriod
    WHEN MATCHED THEN 
        UPDATE SET prs.RatingCount = upd.RatingCount, 
                   prs.UpdatedOn = upd.UpdatedOn
    WHEN NOT MATCHED BY TARGET THEN 
        INSERT (Ukprn, Rating, RatingCount, UpdatedOn,TimePeriod)
        VALUES (upd.Ukprn, upd.Rating, upd.RatingCount, upd.UpdatedOn,@TimePeriod)
        WHEN NOT MATCHED BY SOURCE AND prs.TimePeriod = @TimePeriod THEN
        DELETE;    
    MERGE INTO [dbo].[ProviderStarsSummary] pss
    USING (
    SELECT Ukprn, ReviewCount,
        CASE 
        WHEN AvgRating >= 3.0+@tolerance THEN 4 
        WHEN AvgRating >= 2.0+@tolerance THEN 3 
        WHEN AvgRating >= 1.0+@tolerance THEN 2
        ELSE 1 END Stars
    FROM (
        SELECT Ukprn, SUM(RatingCount) ReviewCount
        ,ROUND(CAST(SUM((CASE [Rating] WHEN 'Very Poor' THEN 1 WHEN 'Poor' THEN 2 WHEN 'Good' THEN 3 WHEN 'Excellent' THEN 4 ELSE 1 END) * RatingCount) as float) / CAST(SUM(RatingCount) as float),1) AvgRating
        FROM [dbo].[ProviderRatingSummary]
		WHERE TimePeriod = @TimePeriod
        GROUP BY Ukprn
        ) av1
    ) upd
    ON pss.Ukprn = upd.Ukprn AND pss.TimePeriod = @TimePeriod
    WHEN MATCHED THEN 
        UPDATE SET pss.ReviewCount = upd.ReviewCount, 
                   pss.Stars = upd.Stars
    WHEN NOT MATCHED BY TARGET THEN 
        INSERT (Ukprn, ReviewCount, Stars,TimePeriod)
        VALUES (upd.Ukprn, upd.ReviewCount, upd.Stars,@TimePeriod)
    WHEN NOT MATCHED BY SOURCE AND pss.TimePeriod = @TimePeriod THEN
        DELETE;    
        FETCH NEXT FROM TimePeriodCursor INTO @TimePeriod;
    END
    CLOSE TimePeriodCursor;
    DEALLOCATE TimePeriodCursor;
END