CREATE PROCEDURE [dbo].[GenerateProviderRatingResults]
(
    @AllUserFeedback INT = 1,        -- Set to 1 (true) to get all user's feedback for each Training provider, or 0 (false) to limit to latest :   The parameter is not being used. It should be removed
    @ResultsforAllTime INT = 0,      -- Set to 1 (true) to get all feedback for each Training provider, or 0 (false) to limit to time period  : The parameter is not being used. It should be removed
    @recentFeedbackMonths INT = 12,  -- Set to limit the time in months to look back for data (default 12, but only actioned if @ResultsforAllTime = 0 : The parameter is not being used. It should be removed
    @tolerance FLOAT = 0.3           -- 0.3 is the current Employer Feedback tolerance, set to 0.5 to match Apprentice feedback : The parameter is not being used. It should be removed
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

DECLARE @ratingTolerance FLOAT = 0.5; 
DECLARE @TimePeriod VARCHAR(10);
DECLARE @StartDate DATETIME;
DECLARE @EndDate DATETIME;
DECLARE @RowNum INT = 1;
DECLARE @TotalRows INT = (SELECT COUNT(*) FROM @TimePeriods);

DELETE FROM [dbo].[ProviderRatingSummary]
WHERE TimePeriod NOT IN (SELECT TimePeriod FROM @TimePeriods);

WHILE @RowNum <= @TotalRows
BEGIN
    SELECT @TimePeriod = TimePeriod, @StartDate = StartDate, @EndDate = EndDate
    FROM @TimePeriods
    WHERE ID = @RowNum;

    IF YEAR(@EndDate) >= 2025
        SET @ratingTolerance = 0.5; -- Tolerance for years starting from 2025
    ELSE
        SET @ratingTolerance = 0.3; -- Default tolerance

	;WITH LatestRatings 
	AS (
	SELECT er1.FeedbackId, er1.ProviderRating, eft.Ukprn
	   FROM (
			SELECT * FROM (
				SELECT ROW_NUMBER() OVER (PARTITION BY FeedbackId ORDER BY DateTimeCompleted DESC) seq, * FROM [dbo].[EmployerFeedbackResult]
				WHERE (DatetimeCompleted BETWEEN @StartDate AND @EndDate)
			) ab1 WHERE (YEAR(@EndDate) < 2025) OR (YEAR(@EndDate) >= 2025 AND seq = 1)
		) er1
		JOIN [dbo].[EmployerFeedback] eft on er1.FeedbackId = eft.FeedbackId
	WHERE (DatetimeCompleted BETWEEN @StartDate AND @EndDate)
	)

    MERGE INTO [dbo].[ProviderRatingSummary] prs
    USING (
        SELECT Ukprn, ProviderRating AS Rating, COUNT(*) AS RatingCount, GETUTCDATE() AS UpdatedOn
        FROM LatestRatings
        GROUP BY Ukprn, ProviderRating
    ) upd
    ON prs.Ukprn = upd.Ukprn AND prs.Rating = upd.Rating AND prs.TimePeriod = @TimePeriod
    WHEN MATCHED THEN 
        UPDATE SET prs.RatingCount = upd.RatingCount, prs.UpdatedOn = upd.UpdatedOn
    WHEN NOT MATCHED BY TARGET THEN 
        INSERT (Ukprn, Rating, RatingCount, UpdatedOn, TimePeriod)
        VALUES (upd.Ukprn, upd.Rating, upd.RatingCount, upd.UpdatedOn, @TimePeriod)
    WHEN NOT MATCHED BY SOURCE AND prs.TimePeriod = @TimePeriod THEN
        DELETE;    

    MERGE INTO [dbo].[ProviderStarsSummary] pss
    USING (
        SELECT Ukprn, ReviewCount,
            CASE 
                WHEN AvgRating >= 3.0 + @ratingTolerance THEN 4 
                WHEN AvgRating >= 2.0 + @ratingTolerance THEN 3 
                WHEN AvgRating >= 1.0 + @ratingTolerance THEN 2
                ELSE 1 
            END  Stars
        FROM (
            SELECT Ukprn, SUM(RatingCount) AS ReviewCount,
                ROUND(CAST(SUM((CASE [Rating] WHEN 'Very Poor' THEN 1 WHEN 'Poor' THEN 2 WHEN 'Good' THEN 3 WHEN 'Excellent' THEN 4 ELSE 1 END) * RatingCount) AS FLOAT) / CAST(SUM(RatingCount) AS FLOAT), 1) AS AvgRating
            FROM [dbo].[ProviderRatingSummary]
            WHERE TimePeriod = @TimePeriod
            GROUP BY Ukprn
        ) av1
    ) upd
    ON pss.Ukprn = upd.Ukprn AND pss.TimePeriod = @TimePeriod
    WHEN MATCHED THEN 
        UPDATE SET pss.ReviewCount = upd.ReviewCount, pss.Stars = upd.Stars
    WHEN NOT MATCHED BY TARGET THEN 
        INSERT (Ukprn, ReviewCount, Stars, TimePeriod)
        VALUES (upd.Ukprn, upd.ReviewCount, upd.Stars, @TimePeriod)
    WHEN NOT MATCHED BY SOURCE AND pss.TimePeriod = @TimePeriod THEN
        DELETE;    

    SET @RowNum += 1;
END

-- Handle 'All' condition outside the loop

BEGIN

DELETE FROM [dbo].[ProviderStarsSummary]
WHERE TimePeriod NOT IN (SELECT TimePeriod FROM @TimePeriods) AND TimePeriod != 'All';

    ;WITH ProviderRatingsWithTolerance AS (
        SELECT
            Ukprn,
            SUM(ReviewCount) AS ReviewCount,
			ROUND(AVG(CAST(Stars AS FLOAT)), 1) AS AvgRating
        FROM
            [dbo].[ProviderStarsSummary]
        WHERE TimePeriod != 'All'
        GROUP BY
            Ukprn
    )
    MERGE INTO [dbo].[ProviderStarsSummary] pss
    USING (
        SELECT 
            Ukprn,
            ReviewCount,
            ROUND(AvgRating, 0) AS Stars
        FROM ProviderRatingsWithTolerance
    ) upd
    ON pss.Ukprn = upd.Ukprn AND pss.TimePeriod = 'All'
    WHEN MATCHED THEN 
        UPDATE SET pss.ReviewCount = upd.ReviewCount, pss.Stars = upd.Stars
    WHEN NOT MATCHED BY TARGET THEN 
        INSERT (Ukprn, ReviewCount, Stars, TimePeriod)
        VALUES (upd.Ukprn, upd.ReviewCount, upd.Stars, 'All')
    WHEN NOT MATCHED BY SOURCE AND pss.TimePeriod = 'All' THEN
        DELETE;
END
END
