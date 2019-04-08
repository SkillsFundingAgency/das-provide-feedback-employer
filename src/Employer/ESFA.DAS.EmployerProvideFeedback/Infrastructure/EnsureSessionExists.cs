using System;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureSessionExists : ActionFilterAttribute
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<EnsureSessionExists> _logger;

        public EnsureSessionExists(ISessionService sessionService, ILogger<EnsureSessionExists> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var uniqueCode = (Guid)context.ActionArguments["uniqueCode"];

             if (!_sessionService.Exists(uniqueCode.ToString()).Result)
            {
                _logger.LogWarning($"Session for code {uniqueCode} does not exist");
                var controller = context.Controller as Controller;
                context.Result = controller.RedirectToRoute(RouteNames.Landing_Get);
            }
        }
    }
}
