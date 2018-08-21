using System;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ReviewAnswersController : Controller
    {
        private readonly ISessionService _sessionService;

        public ReviewAnswersController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet("review-answers", Name = RouteNames.ReviewAnswers_Get)]
        public IActionResult Index(Guid uniqueCode)
        {
            var vm = _sessionService.Get<AnswerModel>(uniqueCode.ToString());
            return View(vm);
        }

        [HttpPost("review-answers", Name = RouteNames.ReviewAnswers_Post)]
        public IActionResult Index(object answers)
        {
            return RedirectToRoute(RouteNames.Confirmation_Get);
        }
    }
}
