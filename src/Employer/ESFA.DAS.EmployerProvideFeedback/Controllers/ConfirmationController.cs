using System;
using System.IO;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Configuration;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Employer.ApplicationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    [ServiceFilter(typeof(EnsureSessionExists))]
    public class ConfirmationController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IGetProviderFeedback _providerFeedbackRepo;
        private readonly ExternalLinksConfiguration _externalLinks;

        public ConfirmationController(ISessionService sessionService, IGetProviderFeedback providerFeedbackRepo, IOptions<ExternalLinksConfiguration> externalLinks)
        {
            _sessionService = sessionService;
            _providerFeedbackRepo = providerFeedbackRepo;
            _externalLinks = externalLinks.Value;
        }

        [HttpGet("feedback-confirmation", Name = RouteNames.Confirmation_Get)]
        public async Task<IActionResult> Index(Guid uniqueCode)
        {
            var surveyModel = await _sessionService.GetAsync<SurveyModel>(uniqueCode.ToString());
            var feedback = await _providerFeedbackRepo.GetProviderFeedbackAsync(surveyModel.Ukprn);

            var confirmationVm = new ConfirmationViewModel
            {
                ProviderName = surveyModel.ProviderName,
                FeedbackRating = surveyModel.Rating.Value,
                Feedback = feedback != null ? new FeedbackViewModel(feedback) : null,
                FatProviderDetailViewUrl = Path.Combine(_externalLinks.FindApprenticeshipTrainingSiteUrl, "Provider", surveyModel.Ukprn.ToString()),
                FatProviderSearch = Path.Combine(_externalLinks.FindApprenticeshipTrainingSiteUrl, "Provider", "Search")
            };

            return View(confirmationVm);
        }
    }
}