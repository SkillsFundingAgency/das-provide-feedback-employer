using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class ProviderSearchViewModel
    {
        [Display(Name="Training provider")]
        public string SelectedProviderName { get; set; }
        public IEnumerable<string> ProviderNameFilter { get; set; }

        [Display(Name = "Feedback status")]
        public string SelectedFeedbackStatus { get; set; }
        public IEnumerable<string> FeedbackStatusFilter { get; set; }

        [Display(Name = "Date submitted")]
        public string SelectedDateSubmitted { get; set; }
        public IEnumerable<string> DateSubmittedFilter { get; set; }
    }
}
