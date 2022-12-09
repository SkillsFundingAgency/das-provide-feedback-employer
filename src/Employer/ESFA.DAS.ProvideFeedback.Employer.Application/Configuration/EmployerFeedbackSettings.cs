
namespace ESFA.DAS.ProvideFeedback.Employer.Application.Configuration
{
    public class EmployerFeedbackSettings
    {
        public int ResultsForAllTime { get; set; }
        public int RecentFeedbackMonths { get; set; }
        public int AllUsersFeedback { get; set; }
        public string Tolerance { get; set; }
    }
}
