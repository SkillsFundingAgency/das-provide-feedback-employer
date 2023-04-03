using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly IEmployerFeedbackRepository _employerEmailDetailsRepository;
        private readonly IEncodingService _encodingService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly IStubAuthenticationService _stubAuthenticationService;


        public HomeController(
            IEmployerFeedbackRepository employerEmailDetailsRepository,
            ISessionService sessionService,
            IEncodingService encodingService,
            ILogger<HomeController> logger,
            IConfiguration config,
            IStubAuthenticationService stubAuthenticationService)
        {
            _employerEmailDetailsRepository = employerEmailDetailsRepository;
            _sessionService = sessionService;
            _encodingService = encodingService;
            _logger = logger;
            _config = config;
            _stubAuthenticationService = stubAuthenticationService;
        }

        [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
        [HttpGet]
        [Route(RoutePrefixPaths.FeedbackRoutePath, Name = RouteNames.Landing_Get_New)]
        public async Task<IActionResult> Index(StartFeedbackRequest request)
        {
            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);   //System.Security.Claims.ClaimTypes.NameIdentifier
            var sessionSurvey = await _sessionService.Get<SurveyModel>(idClaim.Value);
            if (sessionSurvey == null)
            {
                return NotFound();
            }
            else
            {
                ViewData.Add("ProviderName", sessionSurvey.ProviderName);
            }

            return View();
        }
        
        [Authorize(Policy = nameof(PolicyNames.EmployerAuthenticated))]
        [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
        [Route(RoutePrefixPaths.FeedbackFromEmailRoutePath, Name = RouteNames.Landing_Get)]
        [HttpGet]
        public async Task<IActionResult> Index(Guid uniqueCode)
        {
            var idClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);    //System.Security.Claims.ClaimTypes.NameIdentifier

            var employerEmailDetail = await _employerEmailDetailsRepository.GetEmployerInviteForUniqueCode(uniqueCode);

            if (employerEmailDetail == null)
            {
                _logger.LogWarning($"Attempt to use invalid unique code: {uniqueCode}");
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
            newSurveyModel.UniqueCode = uniqueCode;
            await _sessionService.Set(idClaim.Value, newSurveyModel);

            var encodedAccountId = _encodingService.Encode(employerEmailDetail.AccountId, EncodingType.AccountId);
            return RedirectToRoute(RouteNames.Landing_Get_New, new { encodedAccountId = encodedAccountId });
        }

        [Route("signout", Name = RouteNames.Signout)]
        public async Task<IActionResult> SignOut()
        {
            var idToken = await HttpContext.GetTokenAsync("id_token");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add("id_token",idToken);
            return SignOut(
                authenticationProperties, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [Route("signoutcleanup")]
        public void SignOutCleanup()
        {
            Response.Cookies.Delete("SFA.DAS.ProvideFeedbackEmployer.Web.Auth");
        }

        [AllowAnonymous]
        [Route("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
        
#if DEBUG
        [AllowAnonymous()]
        [HttpGet]
        [Route("SignIn-Stub")]
        public IActionResult SigninStub()
        {
            return View("SigninStub", new List<string>{_config["StubId"],_config["StubEmail"]});
        }
        
        [AllowAnonymous()]
        [HttpPost]
        [Route("SignIn-Stub")]
        public IActionResult SigninStubPost()
        {
            _stubAuthenticationService?.AddStubEmployerAuth(Response.Cookies, new StubAuthUserDetails
            {
                Email = _config["StubEmail"],
                Id = _config["StubId"]
            },true);

            return RedirectToRoute("Signed-in-stub");
        }

        [Authorize()]
        [HttpGet]
        [Route("signed-in-stub", Name = "Signed-in-stub")]
        public IActionResult SignedInStub()
        {
            return View();
        }
#endif

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