using System.Collections.Generic;

namespace ESFA.DAS.ProvideFeedback.Domain.Entities.Messages
{
    public class GroupedFeedbackRefreshMessage
    {
        public IEnumerable<EmployerFeedbackRefreshMessage> RefreshMessages { get; set; }
    }
}
