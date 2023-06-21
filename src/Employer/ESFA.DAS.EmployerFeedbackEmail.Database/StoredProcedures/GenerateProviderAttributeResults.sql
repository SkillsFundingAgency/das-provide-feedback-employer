CREATE PROCEDURE [dbo].[GenerateProviderAttributeResults]
(
    @AllUserFeedback INT = 1,        -- Set to 1 (true) to get all user's feedback for each Training provider, or 0 (false) to limit to latest 
    @ResultsforAllTime INT = 1,      -- Set to 1 (true) to get all feedback for each Training provider, or 0 (false) to limit to time period @recentFeedbackMonths
    @recentFeedbackMonths INT = 12   -- Set to limit the time in months to look back for data (default 12, but only actioned if @ResultsforAllTime = 0
)
AS
BEGIN
    
    WITH LatestResults 
    AS (
    SELECT er1.FeedbackId , pa1.AttributeId, pa1.AttributeValue, eft.Ukprn
       FROM (
          -- get latest or all feedback for each feedbackId 
            SELECT * FROM (
                SELECT ROW_NUMBER() OVER (PARTITION BY FeedbackId ORDER BY DateTimeCompleted DESC) seq, * FROM [dbo].[EmployerFeedbackResult]
            ) ab1 WHERE (seq = 1 OR @AllUserFeedback = 1)
        ) er1
        JOIN [dbo].[EmployerFeedback] eft on er1.FeedbackId = eft.FeedbackId
        JOIN [dbo].[ProviderAttributes] pa1 on pa1.EmployerFeedbackResultId = er1.Id
    WHERE (DateTimeCompleted >= DATEADD(MONTH,-@recentFeedbackMonths,GETUTCDATE()) OR @ResultsforAllTime = 1 ) 
    )
    -- Get the ratings for all eligble results for each UKPRNS
    MERGE INTO [dbo].[ProviderAttributeSummary] pas 
    USING (      
    SELECT Ukprn, AttributeId
    , SUM(CASE WHEN AttributeValue = 1 THEN 1 ELSE 0 END) Strength 
    , SUM(CASE WHEN AttributeValue = 1 THEN 0 ELSE 1 END) Weakness 
    , GETUTCDATE() UpdatedOn
    FROM LatestResults
    GROUP BY Ukprn, AttributeId
    ) upd
    ON pas.Ukprn = upd.Ukprn AND pas.AttributeId = upd.AttributeId
    WHEN MATCHED THEN 
        UPDATE SET pas.Strength = upd.Strength, 
                   pas.Weakness = upd.Weakness,
                   pas.UpdatedOn = upd.UpdatedOn
    WHEN NOT MATCHED BY TARGET THEN 
        INSERT (Ukprn, AttributeId, Strength, Weakness, UpdatedOn) 
        VALUES (upd.Ukprn, upd.AttributeId, upd.Strength, upd.Weakness, upd.UpdatedOn)
    WHEN NOT MATCHED BY SOURCE THEN
        DELETE;

END
GO
