using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ESFA.DAS.EmployerProvideFeedback.Authentication
{
    public class EmployerViewerTransactorAuthorizationHandler : AuthorizationHandler<EmployerViewerTransactorRoleRequirement>
    {
       
        private readonly IEmployerAccountAuthorisationHandler _employerAccountAuthorizationHandler;
        
        public EmployerViewerTransactorAuthorizationHandler(IEmployerAccountAuthorisationHandler employerAccountAuthorizationHandler)
        {
            _employerAccountAuthorizationHandler = employerAccountAuthorizationHandler;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerViewerTransactorRoleRequirement employerViewRoleRequirement)
        {
            
            if (!_employerAccountAuthorizationHandler.IsEmployerAuthorised(context, true))
            {
                return Task.CompletedTask;
            }
            
            context.Succeed(employerViewRoleRequirement);

            return Task.CompletedTask;
        }
    }
}