using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Das.ProvideFeedback.Domain.Entities;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class HomeController : Controller
    {
        private readonly IStoreEmployerEmailDetails _employerEmailDetailsRepository;
        private readonly ISessionService _sessionService;
        private readonly List<ProviderAttributeModel> _providerAttributeList;

        public HomeController(IStoreEmployerEmailDetails employerEmailDetailsRepository, ISessionService sessionService, IOptions<List<ProviderAttributeModel>> providerAttributes)
        {
            _employerEmailDetailsRepository = employerEmailDetailsRepository;
            _sessionService = sessionService;
            _providerAttributeList = providerAttributes.Value;
        }

        [HttpGet(Name = RouteNames.Landing_Get)]
        public async Task<IActionResult> Index(Guid uniqueCode)
        {
            var sessionSurvey = _sessionService.Get<SurveyModel>(uniqueCode.ToString());

            if (sessionSurvey == null)
            {
                var employerEmailDetail = await _employerEmailDetailsRepository.GetEmailDetailsForUniqueCode(uniqueCode);
                var newSurveyModel = MapToNewSurveyModel(employerEmailDetail);
                _sessionService.Set(uniqueCode.ToString(), newSurveyModel);
                ViewData.Add("ProviderName", employerEmailDetail.ProviderName);
            }
            else
            {
                ViewData.Add("ProviderName", sessionSurvey.ProviderName);
            }

            return View();
        }

        private SurveyModel MapToNewSurveyModel(EmployerEmailDetail employerEmailDetail)
        {
            return new SurveyModel
            {
                AccountId = employerEmailDetail.AccountId,
                Ukprn = employerEmailDetail.ProviderId,
                UserRef = employerEmailDetail.UserRef,
                Submitted = employerEmailDetail.CodeBurntDate != null,
                ProviderName = employerEmailDetail.ProviderName,
                ProviderAttributes = _providerAttributeList
            };
        }
    }
}