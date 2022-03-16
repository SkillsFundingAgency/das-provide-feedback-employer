using ESFA.DAS.EmployerProvideFeedback.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class ProviderSearchViewModel
    {
        public long AccountId { get; set; }
        public string EncodedAccountId { get; set; }


        [Display(Name = "Training provider")]
        public string SelectedProviderName { get; set; }
        public IEnumerable<string> ProviderNameFilter { get; set; }

        [Display(Name = "Feedback status")]
        public string SelectedFeedbackStatus { get; set; }
        public IEnumerable<string> FeedbackStatusFilter { get; set; }



        public PaginatedList<ProviderSearchViewModel.EmployerTrainingProvider> TrainingProviders { get; set; }

        public string ChangePageAction { get; set; }

        public class EmployerTrainingProvider
        {
            public long ProviderId { get; set; }
            public string ProviderName { get; set; }
            public string FeedbackStatus { get; set; }
            public DateTime? DateSubmitted { get; set; }
        }
    }
}
