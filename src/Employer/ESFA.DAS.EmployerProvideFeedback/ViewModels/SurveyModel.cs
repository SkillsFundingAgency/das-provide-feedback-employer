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
        public List<ProviderAttributeModel> Attributes { get; set; } = new List<ProviderAttributeModel>();

        [Required]
        public ProviderRating? Rating { get; set; }

        public bool HasStrengths => Attributes.Any(attr => attr.Good);

        public bool HasWeaknesses => Attributes.Any(attr => attr.Bad);

        public bool Submitted { get; internal set; }

        public string ProviderName { get; set; }

        public string FatUrl { get; internal set; }

        public Guid? UniqueCode { get; set; }
    }
}