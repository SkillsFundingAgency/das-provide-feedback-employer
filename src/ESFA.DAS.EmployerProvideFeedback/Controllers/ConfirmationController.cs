using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ConfirmationController : Controller
    {
        [HttpGet("feedback-confirmation", Name=RouteNames.Confirmation_Get)]
        public IActionResult Index()
        {
            return View();
        }
    }
}
