using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Services;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Authorize]
    public class ProviderController : Controller
    {
        private readonly IEmployerFeedbackRepository _employerEmailDetailsRepository;
        private readonly ISessionService _sessionService;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly ILogger<ProviderController> _logger;
        private readonly IEncodingService _encodingService;

        private const int DefaultPageIndex = 1;
        private const int DefaultPageSize = 2;

        public ProviderController(
            IEmployerFeedbackRepository employerEmailDetailsRepository
            , ISessionService sessionService
            , ITrainingProviderService trainingProviderService
            , IEncodingService encodingService
            , ILogger<ProviderController> logger
            )
        {
            _employerEmailDetailsRepository = employerEmailDetailsRepository;
            _sessionService = sessionService;
            _trainingProviderService = trainingProviderService;
            _encodingService = encodingService;
            _logger = logger;
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Index(GetProvidersForFeedbackRequest request, int pageIndex = DefaultPageIndex)
        {
            var model = await _trainingProviderService.GetTrainingProviderSearchViewModel(
                request.EncodedAccountId, 
                string.Empty,
                string.Empty,
                DefaultPageSize,
                pageIndex);
            model.ChangePageAction = nameof(Index);



            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);




            if (model.TrainingProviders.TotalRecordCount == 0)
            {
                // Can this happen in prod?
            }
            else if (model.TrainingProviders.TotalRecordCount == 1)
            {
                // Go straight to Start
            }

            return View(model);
        }

        [HttpPost]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Filter(ProviderSearchViewModel postedModel)
        {
            var model = await _trainingProviderService.GetTrainingProviderSearchViewModel(
                postedModel.EncodedAccountId,
                postedModel.SelectedProviderName,
                postedModel.SelectedFeedbackStatus,
                DefaultPageSize,
                DefaultPageIndex);  // applying filter resets the paging

            return View("Index", model);
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers/{providerId}")]
        public async Task<IActionResult> ConfirmProvider(string encodedAccountId, long providerId)
        {
            var model = await _trainingProviderService.GetTrainingProviderConfirmationViewModel(encodedAccountId, providerId);

            return View(model);
        }

        [HttpPost]
        [Route("/{encodedAccountId}/providers/{providerId}")]
        public async Task<IActionResult> ProviderConfirmed(ProviderSearchConfirmationViewModel postedModel)
        {            
            if(!postedModel.Confirmed.HasValue)
            {
                ModelState.AddModelError("Confirmation", "Please choose an option");
                return View("ConfirmProvider", postedModel);
            }

            if(!postedModel.Confirmed.Value)
            {
                return RedirectToAction("Index");
            }

            var providerAttributes = await _employerEmailDetailsRepository.GetAllAttributes();
            if (providerAttributes == null)
            {
                _logger.LogError($"Unable to load Provider Attributes from the database.");
                return RedirectToAction("Error", "Error");
            }

            var providerAttributesModel = providerAttributes.Select(s => new ProviderAttributeModel { Name = s.AttributeName }).ToList();

            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            var newSurveyModel = new SurveyModel
            {
                AccountId = _encodingService.Decode(postedModel.EncodedAccountId, EncodingType.AccountId),
                Ukprn = postedModel.ProviderId,
                UserRef = new Guid(idClaim?.Value), 
                Submitted = false, //employerEmailDetail.CodeBurntDate != null,
                ProviderName = postedModel.ProviderName,
                Attributes = providerAttributesModel
            };

            await _sessionService.Set(idClaim.Value, newSurveyModel);

            return RedirectToAction("Index", "Home");
        }
    }
}
