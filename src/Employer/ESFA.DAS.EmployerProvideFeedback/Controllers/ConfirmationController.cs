using System;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Authorize]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    [ServiceFilter(typeof(EnsureSessionExists))]
    public class ConfirmationController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<ConfirmationController> _logger;
        private readonly ExternalLinksConfiguration _externalLinks;

        public ConfirmationController(
            ISessionService sessionService,  
            IOptions<ExternalLinksConfiguration> externalLinks,
            ILogger<ConfirmationController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
            _externalLinks = externalLinks.Value;
        }

        [HttpGet("feedback-confirmation", Name = RouteNames.Confirmation_Get)]
        public async Task<IActionResult> Index(string encodedAccountId)
        {
            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var surveyModel = await _sessionService.Get<SurveyModel>(idClaim.Value);
            var providerCount = await _sessionService.Get<int>($"{idClaim.Value}_ProviderCount");
            var hasMultipleProviders = providerCount > 0;

            var confirmationVm = new ConfirmationViewModel
            {
                ProviderName = surveyModel.ProviderName,
                FeedbackRating = surveyModel.Rating.Value,
                FatUrl = _externalLinks.FindApprenticeshipTrainingSiteUrl,
                HasMultipleProviders = hasMultipleProviders,
                EncodedAccountId = encodedAccountId
            };

            return View(confirmationVm);
        }
    }
}