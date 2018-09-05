using System;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ConfirmationController : Controller
    {
        private readonly ISessionService _sessionService;

        public ConfirmationController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet("feedback-confirmation", Name = RouteNames.Confirmation_Get)]
        public IActionResult Index(Guid uniqueCode)
        {
            var surveyModel = _sessionService.Get<SurveyModel>(uniqueCode.ToString());
            return View(surveyModel);
        }
    }
}