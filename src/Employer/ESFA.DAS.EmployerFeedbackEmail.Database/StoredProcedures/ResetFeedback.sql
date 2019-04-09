CREATE PROCEDURE [dbo].[ResetFeedback]
AS
	UPDATE [dbo].[EmployerFeedback] SET IsActive = 0