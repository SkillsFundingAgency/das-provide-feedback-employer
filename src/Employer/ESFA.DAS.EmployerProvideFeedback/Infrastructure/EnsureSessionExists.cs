using System;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureSessionExists : ActionFilterAttribute
    {
        private readonly ISessionService _sessionService;

        public EnsureSessionExists(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        public async override void OnActionExecuting(ActionExecutingContext context)
        {
            var uniqueCode = (Guid)context.ActionArguments["uniqueCode"];

            if (!await _sessionService.ExistsAsync(uniqueCode.ToString()))
            {
                var controller = context.Controller as Controller;
                context.Result = controller.RedirectToRoute(RouteNames.Landing_Get);
            }
        }
    }
}
