CREATE PROCEDURE [dbo].[ResetFeedback]
AS
	UPDATE [dbo].[Feedback] SET IsActive = 0