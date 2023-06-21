CREATE PROCEDURE [dbo].[GenerateProviderRatingResults]
(
    @AllUserFeedback INT = 1,        -- Set to 1 (true) to get all user's feedback for each Training provider, or 0 (false) to limit to latest 
    @ResultsforAllTime INT = 1,      -- Set to 1 (true) to get all feedback for each Training provider, or 0 (false) to limit to time period @recentFeedbackMonths
    @recentFeedbackMonths INT = 12,  -- Set to limit the time in months to look back for data (default 12, but only actioned if @ResultsforAllTime = 0
    @tolerance FLOAT = 0.3           -- 0.3 is the current Employer Feedback tolerance, set to 0.5 to match Apprentice feedback
)
AS
BEGIN
    
    WITH LatestRatings 
    AS (
    SELECT er1.FeedbackId, er1.ProviderRating, eft.Ukprn
       FROM (
          -- get latest or all feedback for each feedbackId 
            SELECT * FROM (
                SELECT ROW_NUMBER() OVER (PARTITION BY FeedbackId ORDER BY DateTimeCompleted DESC) seq, * FROM [dbo].[EmployerFeedbackResult]
            ) ab1 WHERE (seq = 1 OR @AllUserFeedback = 1)
        ) er1
        JOIN [dbo].[EmployerFeedback] eft on er1.FeedbackId = eft.FeedbackId
    WHERE (DateTimeCompleted >= dateadd(month,-@recentFeedbackMonths,getdate()) OR @ResultsforAllTime = 1 )
    )
    -- Get the ratings for all eligble results for each UKPRNS
    MERGE INTO [dbo].[ProviderRatingSummary]  prs
    USING (    
    SELECT Ukprn, ProviderRating Rating, count(*) RatingCount, GETUTCDATE() UpdatedOn
    FROM LatestRatings
    GROUP BY Ukprn, ProviderRating
    ) upd
    ON prs.Ukprn = upd.Ukprn AND prs.Rating = upd.Rating
    WHEN MATCHED THEN 
        UPDATE SET prs.RatingCount = upd.RatingCount, 
                   prs.UpdatedOn = upd.UpdatedOn
    WHEN NOT MATCHED BY TARGET THEN 
        INSERT (Ukprn, Rating, RatingCount, UpdatedOn)
        VALUES (upd.Ukprn, upd.Rating, upd.RatingCount, upd.UpdatedOn)
    WHEN NOT MATCHED BY SOURCE THEN
        DELETE;    

    -- summarise the rating summary to get the Stars
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
        GROUP BY Ukprn
        ) av1
    ) upd
    ON pss.Ukprn = upd.Ukprn
    WHEN MATCHED THEN 
        UPDATE SET pss.ReviewCount = upd.ReviewCount, 
                   pss.Stars = upd.Stars
    WHEN NOT MATCHED BY TARGET THEN 
        INSERT (Ukprn, ReviewCount, Stars)
        VALUES (upd.Ukprn, upd.ReviewCount, upd.Stars)
    WHEN NOT MATCHED BY SOURCE THEN
        DELETE;    
    
END
GO
