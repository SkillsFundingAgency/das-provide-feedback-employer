using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class AnswerModel
    {
        public List<ProviderAttributeModel> ProviderAttributes { get; set; } = new List<ProviderAttributeModel>();

        [Required]
        public ProviderRating? ProviderRating { get; set; }

        public bool HasStrengths => ProviderAttributes.Any(attr => attr.IsDoingWell);

        public bool HasWeaknesses => ProviderAttributes.Any(attr => attr.IsToImprove);

        public bool Submitted { get; internal set; }
    }
}
