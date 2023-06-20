using System;
using System.Security.Claims;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
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
            var c = context.Controller as Controller;
            var userId = c.User.FindFirstValue(EmployerClaims.UserId);

             if (!_sessionService.ExistsAsync(userId).Result)
            {
                _logger.LogWarning($"Session for user id {userId} does not exist");
                var controller = context.Controller as Controller;
                context.Result = controller.RedirectToRoute(RouteNames.Landing_Get);
            }
        }
    }
}
