using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class AnswerModel
    {
        public List<ProviderAttribute> ProviderAttributes { get; set; } = new List<ProviderAttribute>();

        [Required]
        public ProviderRating? ProviderRating { get; set; }

        public bool HasStrengths => ProviderAttributes.Any(attr => attr.IsDoingWell);

        public bool HasWeaknesses => ProviderAttributes.Any(attr => attr.IsToImprove);
    }
}
