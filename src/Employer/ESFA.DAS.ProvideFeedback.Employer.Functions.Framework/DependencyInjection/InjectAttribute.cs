using System;
using Microsoft.Azure.WebJobs.Description;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Framework.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [Binding]
    public sealed class InjectAttribute : Attribute
    {
    }
}
