namespace ESFA.DAS.EmployerProvideFeedback.Api.Models
{
    using System;
    using System.Collections.Generic;

    public interface IEmployerFeedback
    {
        long Ukprn { get; set; }

        DateTime DateTimeCompleted { get; set; }

        List<IProviderAttribute> ProviderAttributes { get; set; }

        string ProviderRating { get; set; }
    }
}