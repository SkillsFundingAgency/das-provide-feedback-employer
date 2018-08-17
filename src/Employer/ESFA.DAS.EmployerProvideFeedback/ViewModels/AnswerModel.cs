using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ESFA.DAS.EmployerProvideFeedback.ViewModels
{
    public class AnswerModel
    {
        public List<ProviderSkill> ProviderSkills { get; set; } = new List<ProviderSkill>();

        [Required]
        public ProviderRating? ProviderRating { get; set; }

        public bool HasStrengths => ProviderSkills.Any(skill => skill.IsDoingWell);

        public bool HasWeaknesses => ProviderSkills.Any(skill => skill.IsToImprove);
    }
}
