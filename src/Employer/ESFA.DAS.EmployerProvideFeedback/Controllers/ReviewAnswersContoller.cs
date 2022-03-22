using System;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Orchestrators;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(EnsureFeedbackNotSubmittedRecently))]
    [ServiceFilter(typeof(EnsureSessionExists))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ReviewAnswersController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly ReviewAnswersOrchestrator _orchestrator;
        private readonly ProvideFeedbackEmployerWeb _config;

        public ReviewAnswersController(
            ISessionService sessionService
            , ReviewAnswersOrchestrator orchestrator
            , ProvideFeedbackEmployerWeb config
            )
        {
            _sessionService = sessionService;
            _orchestrator = orchestrator;
            _config = config;
        }

        [HttpGet("review-answers", Name = RouteNames.ReviewAnswers_Get)]
        public async Task<IActionResult> Index(Guid uniqueCode)
        {
            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var vm = await _sessionService.Get<SurveyModel>(idClaim.Value);
            vm.FatUrl = _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl;
            return View(vm);
        }

        [HttpPost("review-answers", Name = RouteNames.ReviewAnswers_Post)]
        public async Task<IActionResult> Confirmation(Guid uniqueCode)
        {
            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            var answers = await _sessionService.Get<SurveyModel>(idClaim.Value);

            answers.Submitted = true;
            await _orchestrator.SubmitConfirmedEmployerFeedback(answers, uniqueCode);
            await _sessionService.Set(idClaim.Value, answers);

            return RedirectToRoute(RouteNames.Confirmation_Get);
        }
    }
}
