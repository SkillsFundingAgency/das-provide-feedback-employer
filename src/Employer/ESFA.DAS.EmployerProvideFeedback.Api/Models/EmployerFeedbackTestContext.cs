using ESFA.DAS.FeedbackDataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    public class EmployerFeedbackTestContext : DbContext
    {
        public EmployerFeedbackTestContext(DbContextOptions<EmployerFeedbackTestContext> options)
            : base(options)
        {
        }

        public DbSet<FeedbackDataAccess.Models.EmployerFeedback> EmployerFeedback { get; set; }

    }
}
