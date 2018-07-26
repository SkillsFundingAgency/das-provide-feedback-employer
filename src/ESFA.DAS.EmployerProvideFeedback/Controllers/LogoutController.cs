using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Route("")]
    public class LogoutController : Controller
    {
        private readonly ExternalLinksConfiguration _externalLinks;

        public LogoutController(IOptions<ExternalLinksConfiguration> externalLinksOptions)
        {
            _externalLinks = externalLinksOptions.Value;
        }

        [HttpGet, Route("logout", Name = RouteNames.Logout_Get)]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");

            return Redirect(_externalLinks.ManageApprenticeshipSiteUrl);
        }
    }
}