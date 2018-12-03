using System;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.DependencyInjection
{
    public interface IServiceProviderBuilder
    {
        IServiceProvider Build();
    }
}
