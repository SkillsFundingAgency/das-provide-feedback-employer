using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure.Validation;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class SurveyModel
    {
        public Guid UserRef { get; set; }

        public long Ukprn { get; set; }

        public long AccountId { get; set; }

        [EnsureMaxThreeProviderAttribute]
        public List<ProviderAttributeModel> ProviderAttributes { get; set; } = new List<ProviderAttributeModel>();

        [Required]
        public ProviderRating? ProviderRating { get; set; }

        public bool HasStrengths => ProviderAttributes.Any(attr => attr.IsDoingWell);

        public bool HasWeaknesses => ProviderAttributes.Any(attr => attr.IsToImprove);

        public bool Submitted { get; internal set; }

        public string ProviderName { get; set; }
    }
}