using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{

    public class HomeController : Controller
    {
        private readonly IEmployerFeedbackRepository _employerEmailDetailsRepository;
        private readonly ISessionService _sessionService;
        private readonly ILogger<HomeController> _logger;
        

        public HomeController(
            IEmployerFeedbackRepository employerEmailDetailsRepository,
            ISessionService sessionService,
            ILogger<HomeController> logger)
        {
            _employerEmailDetailsRepository = employerEmailDetailsRepository;
            _sessionService = sessionService;
            _logger = logger;
        }

        [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
        [Route(RoutePrefixPaths.FeedbackRoutePath, Name = RouteNames.Landing_Get)]
        [HttpGet]
        public async Task<IActionResult> Index(Guid uniqueCode)
        {
            var sessionSurvey = await _sessionService.Get<SurveyModel>(uniqueCode.ToString());

            if (sessionSurvey == null)
            {
                var employerEmailDetail = await _employerEmailDetailsRepository.GetEmployerInviteForUniqueCode(uniqueCode);

                if (employerEmailDetail == null)
                {
                    _logger.LogWarning($"Attempt to use invalid unique code: {uniqueCode}");
                    //TODO: 
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
                await _sessionService.Set(uniqueCode.ToString(), newSurveyModel);
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