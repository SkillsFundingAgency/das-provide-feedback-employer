using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Das.ProvideFeedback.Domain.Entities;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly IStoreEmployerEmailDetails _employerEmailDetailsRepository;
        private readonly ISessionService _sessionService;
        private readonly ILogger<HomeController> _logger;
        private readonly List<ProviderAttributeModel> _providerAttributeList;

        public HomeController(
            IStoreEmployerEmailDetails employerEmailDetailsRepository,
            ISessionService sessionService,
            ILogger<HomeController> logger,
            IOptions<List<ProviderAttributeModel>> providerAttributes)
        {
            _employerEmailDetailsRepository = employerEmailDetailsRepository;
            _sessionService = sessionService;
            _logger = logger;
            _providerAttributeList = providerAttributes.Value;
        }

        [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
        [Route(RoutePrefixPaths.FeedbackRoutePath, Name = RouteNames.Landing_Get)]
        [HttpGet]
        public async Task<IActionResult> Index(Guid uniqueCode)
        {
            var sessionSurvey = await _sessionService.GetAsync<SurveyModel>(uniqueCode.ToString());

            if (sessionSurvey == null)
            {
                var employerEmailDetail = await _employerEmailDetailsRepository.GetEmailDetailsForUniqueCode(uniqueCode);

                if (employerEmailDetail == null)
                {
                    _logger.LogWarning($"Attempt to use invaliid unique code: {uniqueCode}");
                    //TODO: 
                    return NotFound();
                }

                var newSurveyModel = MapToNewSurveyModel(employerEmailDetail);
                await _sessionService.SetAsync(uniqueCode.ToString(), newSurveyModel);
                ViewData.Add("ProviderName", employerEmailDetail.ProviderName);
            }
            else
            {
                ViewData.Add("ProviderName", sessionSurvey.ProviderName);
            }

            return View();
        }

        [HttpGet(Name = RouteNames.Cookies)]
        public IActionResult Cookies()
        {
            return View();
        }


        [HttpGet(Name = RouteNames.Privacy)]
        public IActionResult Privacy()
        {
            return View();
        }

        private SurveyModel MapToNewSurveyModel(EmployerEmailDetail employerEmailDetail)
        {
            return new SurveyModel
            {
                AccountId = employerEmailDetail.AccountId,
                Ukprn = employerEmailDetail.Ukprn,
                UserRef = employerEmailDetail.UserRef,
                Submitted = employerEmailDetail.CodeBurntDate != null,
                ProviderName = employerEmailDetail.ProviderName,
                Attributes = _providerAttributeList
            };
        }
    }
}