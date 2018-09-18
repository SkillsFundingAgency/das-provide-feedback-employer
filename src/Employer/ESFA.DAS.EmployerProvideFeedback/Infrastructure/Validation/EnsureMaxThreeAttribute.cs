using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.FeedbackDataAccess.Models;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure.Validation
{
    public class EnsureMaxThreeProviderAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var list = value as List<ProviderAttributeModel>;
            if (list != null)
            {
                
                return list.Where(att => att.IsDoingWell).Count() <= 3
                    && list.Where(att => att.IsToImprove).Count() <= 3;
            }

            return false;
        }
    }
}
