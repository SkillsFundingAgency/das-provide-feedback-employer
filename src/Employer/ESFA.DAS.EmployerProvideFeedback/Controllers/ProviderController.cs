using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.Paging;
using ESFA.DAS.EmployerProvideFeedback.Services;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.Employer.Shared.UI;
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
        private readonly UrlBuilder _urlBuilder;

        public ProviderController(
            IEmployerFeedbackRepository employerEmailDetailsRepository
            , ISessionService sessionService
            , ITrainingProviderService trainingProviderService
            , IEncodingService encodingService
            , ILogger<ProviderController> logger
            , UrlBuilder urlBuilder
            )
        {
            _employerEmailDetailsRepository = employerEmailDetailsRepository;
            _sessionService = sessionService;
            _trainingProviderService = trainingProviderService;
            _encodingService = encodingService;
            _logger = logger;
            _urlBuilder = urlBuilder;
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Index(GetProvidersForFeedbackRequest request, int pageIndex = PagingState.DefaultPageIndex)
        {
            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var pagingState = await _sessionService.Get<PagingState>($"{idClaim.Value}_PagingState");
            if(null == pagingState)
            {
                pagingState = new PagingState();
            }
            pagingState.PageIndex = pageIndex;
            await _sessionService.Set($"{idClaim.Value}_PagingState", pagingState);

            var model = await _trainingProviderService.GetTrainingProviderSearchViewModel(
                request.EncodedAccountId,
                pagingState.SelectedProviderName,
                pagingState.SelectedFeedbackStatus,
                pagingState.PageSize,
                pagingState.PageIndex,
                pagingState.SortColumn,
                pagingState.SortDirection);
            model.ChangePageAction = nameof(Index);

            ViewBag.EmployerAccountsHomeUrl = _urlBuilder.AccountsLink("AccountsHome", request.EncodedAccountId);

            await _sessionService.Set($"{idClaim.Value}_ProviderCount", model.TrainingProviders.TotalRecordCount);

            if (model.TrainingProviders.TotalRecordCount == 1)
            {
                // Go straight to Start
            }

            return View(model);
        }

        [HttpPost]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Filter(ProviderSearchViewModel postedModel)
        {
            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var pagingState = await _sessionService.Get<PagingState>($"{idClaim.Value}_PagingState");
            if (null == pagingState)
            {
                pagingState = new PagingState();
            }
            pagingState.PageIndex = PagingState.DefaultPageIndex; // applying filter resets the paging
            pagingState.SelectedProviderName = postedModel.SelectedProviderName;
            pagingState.SelectedFeedbackStatus = postedModel.SelectedFeedbackStatus;
            await _sessionService.Set($"{idClaim.Value}_PagingState", pagingState);

            var model = await _trainingProviderService.GetTrainingProviderSearchViewModel(
                postedModel.EncodedAccountId,
                pagingState.SelectedProviderName,
                pagingState.SelectedFeedbackStatus,
                pagingState.PageSize,
                pagingState.PageIndex,
                pagingState.SortColumn,
                pagingState.SortDirection);  

            return View("Index", model);
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers/sort")]
        public async Task<IActionResult> SortProviders(string encodedAccountId, string sortColumn, string sortDirection)
        {
            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var pagingState = await _sessionService.Get<PagingState>($"{idClaim.Value}_PagingState");
            if (null == pagingState)
            {
                pagingState = new PagingState();
            }
            pagingState.SortColumn = sortColumn;
            pagingState.SortDirection = sortDirection;
            await _sessionService.Set($"{idClaim.Value}_PagingState", pagingState);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("/{encodedAccountId}/providers/unfilter")]
        public async Task<IActionResult> ClearFilters(string encodedAccountId)
        {
            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var pagingState = await _sessionService.Get<PagingState>($"{idClaim.Value}_PagingState");
            if (null == pagingState)
            {
                pagingState = new PagingState();
            }
            pagingState.SelectedProviderName = string.Empty;
            pagingState.SelectedFeedbackStatus = string.Empty;
            await _sessionService.Set($"{idClaim.Value}_PagingState", pagingState);

            return RedirectToAction(nameof(Index));
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

            // Make sure the user exists.
            var emailAddressClaim = HttpContext.User.FindFirst("http://das/employer/identity/claims/email_address");
            var firstNameClaim = HttpContext.User.FindFirst("http://das/employer/identity/claims/given_name");
            var user = new User()
            {
                UserRef = new Guid(idClaim?.Value),
                EmailAddress = emailAddressClaim?.Value,
                FirstName = firstNameClaim?.Value,
            };
            await _employerEmailDetailsRepository.UpsertIntoUsers(user);

            // Make sure the provider exists and is active.
            await _trainingProviderService.UpsertTrainingProvider(newSurveyModel.Ukprn, newSurveyModel.ProviderName);

            return RedirectToAction("Index", "Home");
        }
    }
}
