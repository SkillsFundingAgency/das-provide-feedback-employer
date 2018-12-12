using ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection.Config
{
    internal class InjectConfiguration : IExtensionConfigProvider
    {
        public readonly InjectBindingProvider _injectBindingProvider;

        public InjectConfiguration(InjectBindingProvider injectBindingProvider) =>
            _injectBindingProvider = injectBindingProvider;

        public void Initialize(ExtensionConfigContext context) => context
                .AddBindingRule<InjectAttribute>()
                .Bind(_injectBindingProvider);
    }
}
