using System;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Orchestrators;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ReviewAnswersController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly ReviewAnswersOrchestrator _orchestrator;

        public ReviewAnswersController(ISessionService sessionService, ReviewAnswersOrchestrator orchestrator)
        {
            _sessionService = sessionService;
            _orchestrator = orchestrator;
        }

        [HttpGet("review-answers", Name = RouteNames.ReviewAnswers_Get)]
        public IActionResult Index(Guid uniqueCode)
        {
            var vm = _sessionService.Get<AnswerModel>(uniqueCode.ToString());
            return View(vm);
        }

        [HttpPost("review-answers", Name = RouteNames.ReviewAnswers_Post)]
        public async Task<IActionResult> Confirmation(Guid uniqueCode)
        {
            var answers = _sessionService.Get<AnswerModel>(uniqueCode.ToString());

            answers.Submitted = true;
            await _orchestrator.SubmitConfirmedEmployerFeedback(answers);
            _sessionService.Set(uniqueCode.ToString(), answers);

            return RedirectToRoute(RouteNames.Confirmation_Get);
        }
    }
}
