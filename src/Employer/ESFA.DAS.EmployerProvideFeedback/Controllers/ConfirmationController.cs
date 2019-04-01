using System;
using System.IO;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Apprenticeships.Api.Types.Providers;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    [ServiceFilter(typeof(EnsureSessionExists))]
    public class ConfirmationController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IGetProviderFeedback _providerFeedbackRepo;
        private readonly ILogger<ConfirmationController> _logger;
        private readonly ExternalLinksConfiguration _externalLinks;

        public ConfirmationController(
            ISessionService sessionService, 
            IGetProviderFeedback providerFeedbackRepo, 
            IOptions<ExternalLinksConfiguration> externalLinks,
            ILogger<ConfirmationController> logger)
        {
            _sessionService = sessionService;
            _providerFeedbackRepo = providerFeedbackRepo;
            _logger = logger;
            _externalLinks = externalLinks.Value;
        }

        [HttpGet("feedback-confirmation", Name = RouteNames.Confirmation_Get)]
        public async Task<IActionResult> Index(Guid uniqueCode)
        {
            var surveyModel = await _sessionService.Get<SurveyModel>(uniqueCode.ToString());
            Feedback previousFeedback = null;

            try
            {
                previousFeedback = await _providerFeedbackRepo.GetProviderFeedbackAsync(surveyModel.Ukprn);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Feedback has been given for a ukprn which is invalid according to the ApprenticeshipServiceApi");
            }

            var confirmationVm = new ConfirmationViewModel
            {
                ProviderName = surveyModel.ProviderName,
                FeedbackRating = surveyModel.Rating.Value,
                Feedback = previousFeedback != null ? new FeedbackViewModel(previousFeedback) : null,
                FatProviderDetailViewUrl = Path.Combine(_externalLinks.FindApprenticeshipTrainingSiteUrl, "Provider", surveyModel.Ukprn.ToString()),
                FatProviderSearch = Path.Combine(_externalLinks.FindApprenticeshipTrainingSiteUrl, "Provider", "Search")
            };

            return View(confirmationVm);
        }
    }
}