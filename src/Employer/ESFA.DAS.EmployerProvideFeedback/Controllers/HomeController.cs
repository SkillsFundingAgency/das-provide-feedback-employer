using System;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class HomeController : Controller
    {
        [HttpGet(Name=RouteNames.Landing_Get)]
        public IActionResult Index(Guid uniqueCode)
        {
            return View();
        }
    }
}
