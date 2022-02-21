using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Encoding;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmployerFeedbackRepository _employerEmailDetailsRepository;
        private readonly IEncodingService _encodingService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<HomeController> _logger;


        public HomeController(
            IEmployerFeedbackRepository employerEmailDetailsRepository,
            ISessionService sessionService,
            IEncodingService encodingService,
            ILogger<HomeController> logger)
        {
            _employerEmailDetailsRepository = employerEmailDetailsRepository;
            _sessionService = sessionService;
            _encodingService = encodingService;
            _logger = logger;
        }

        [HttpGet]
        //[Route("{encodedAccountId}/{uniqueCode:guid}")]
        [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
        [Route(RoutePrefixPaths.FeedbackLandingPageRoutePath, Name = RouteNames.Landing_Get_New)]
        public async Task<IActionResult> Index(StartFeedbackRequest request)
        {
            var sessionSurvey = await _sessionService.Get<SurveyModel>(request.UniqueCode.ToString());

            if (sessionSurvey == null)
            {
                var employerEmailDetail = await _employerEmailDetailsRepository.GetEmployerInviteForUniqueCode(request.UniqueCode);

                if (employerEmailDetail == null)
                {
                    _logger.LogWarning($"Attempt to use invalid unique code: {request.UniqueCode}");
                    return NotFound();
                }
                var providerAttributes = await _employerEmailDetailsRepository.GetAllAttributes();
                if (providerAttributes == null)
                {
                    _logger.LogError($"Unable to load Provider Attributes from the database.");
                    return RedirectToAction("Error", "Error");
                }

                var providerAttributesModel = providerAttributes.Select(s => new ProviderAttributeModel { Name = s.AttributeName });
                var newSurveyModel = MapToNewSurveyModel(employerEmailDetail, providerAttributesModel);
                await _sessionService.Set(request.UniqueCode.ToString(), newSurveyModel);
                ViewData.Add("ProviderName", employerEmailDetail.ProviderName);
            }
            else
            {
                ViewData.Add("ProviderName", sessionSurvey.ProviderName);
            }

            return View();
        }

        [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
        [Route(RoutePrefixPaths.FeedbackRoutePath, Name = RouteNames.Landing_Get)]
        [HttpGet]
        public async Task<IActionResult> Index(Guid uniqueCode)
        {
            //var sessionSurvey = await _sessionService.Get<SurveyModel>(uniqueCode.ToString());

            //if (sessionSurvey == null)
            //{
            //    var employerEmailDetail = await _employerEmailDetailsRepository.GetEmployerInviteForUniqueCode(uniqueCode);

            //    if (employerEmailDetail == null)
            //    {
            //        _logger.LogWarning($"Attempt to use invalid unique code: {uniqueCode}");
            //        return NotFound();
            //    }
            //    var providerAttributes = await _employerEmailDetailsRepository.GetAllAttributes();
            //    if (providerAttributes == null)
            //    {
            //        _logger.LogError($"Unable to load Provider Attributes from the database.");
            //        return RedirectToAction("Error", "Error");
            //    }

            //    var providerAttributesModel = providerAttributes.Select(s => new ProviderAttributeModel { Name = s.AttributeName });
            //    var newSurveyModel = MapToNewSurveyModel(employerEmailDetail, providerAttributesModel);
            //    await _sessionService.Set(uniqueCode.ToString(), newSurveyModel);
            //    ViewData.Add("ProviderName", employerEmailDetail.ProviderName);
            //}
            //else
            //{
            //    ViewData.Add("ProviderName", sessionSurvey.ProviderName);
            //}

            //return View();

            //This method becomes the below once the routing is sorted out.
            var sessionSurvey = await _sessionService.Get<SurveyModel>(uniqueCode.ToString());
            long accountId;
            if (sessionSurvey == null)
            {
                var employerEmailDetail = await _employerEmailDetailsRepository.GetEmployerInviteForUniqueCode(uniqueCode);

                if (employerEmailDetail == null)
                {
                    _logger.LogWarning($"Attempt to use invalid unique code: {uniqueCode}");
                    return NotFound();
                }

                accountId = employerEmailDetail.AccountId;
            }
            else
            {
                accountId = sessionSurvey.AccountId;
            }

            var encodedAccountId = _encodingService.Encode(accountId, EncodingType.AccountId);
            // Need to setup route or rework how this is executed.
            return RedirectToRoute(RouteNames.Landing_Get_New, new { encodedAccountId = encodedAccountId, uniqueCode = uniqueCode });
        }

        private SurveyModel MapToNewSurveyModel(EmployerSurveyInvite employerEmailDetail, IEnumerable<ProviderAttributeModel> providerAttributes)
        {
            return new SurveyModel
            {
                AccountId = employerEmailDetail.AccountId,
                Ukprn = employerEmailDetail.Ukprn,
                UserRef = employerEmailDetail.UserRef,
                Submitted = employerEmailDetail.CodeBurntDate != null,
                ProviderName = employerEmailDetail.ProviderName,
                Attributes = providerAttributes.ToList()
            };
        }
    }
}