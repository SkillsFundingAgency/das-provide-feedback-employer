using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;

namespace ESFA.DAS.ProvideFeedback.Employer.Functions.Emailer.DependencyInjection.Services
{
    internal class ScopeCleanupFilter : IFunctionInvocationFilter, IFunctionExceptionFilter
    {
        private readonly ServiceProviderHolder _scopeHolder;

        public ScopeCleanupFilter(ServiceProviderHolder scopeHolder) =>
            _scopeHolder = scopeHolder;

        public Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            _scopeHolder.RemoveScope(exceptionContext.FunctionInstanceId);
            return Task.CompletedTask;
        }

        public Task OnExecutedAsync(FunctionExecutedContext executedContext, CancellationToken cancellationToken)
        {
            _scopeHolder.RemoveScope(executedContext.FunctionInstanceId);
            return Task.CompletedTask;
        }

        public Task OnExecutingAsync(FunctionExecutingContext executingContext, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
