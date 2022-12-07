using System;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
        [Route(RoutePrefixPaths.FeedbackFromEmailRoutePath, Name = RouteNames.Landing_Get)]
        [HttpGet]
        public async Task<IActionResult> Index(Guid uniqueCode)
        {
            var employerEmailDetail = await _employerEmailDetailsRepository.GetEmployerAccountIdFromUniqueSurveyCode(uniqueCode);

            if (employerEmailDetail == 0)
            {
                _logger.LogWarning($"Attempt to use invalid unique code: {uniqueCode}");
                return NotFound();
            }

            var encodedAccountId = _encodingService.Encode(employerEmailDetail, EncodingType.AccountId);

            return RedirectToRoute(RouteNames.ProviderSelect, new { encodedAccountId = encodedAccountId });
        }

        [Route("signout", Name = RouteNames.Signout)]
        public IActionResult SignOut()
        {
            return SignOut(
                new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                {
                    RedirectUri = "",
                    AllowRefresh = true
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
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
    }
}