using System;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection
{
    public interface IServiceProviderBuilder
    {
        IServiceProvider Build();
    }
}
