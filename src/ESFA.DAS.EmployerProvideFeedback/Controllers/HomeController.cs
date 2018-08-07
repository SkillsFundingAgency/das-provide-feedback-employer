using Microsoft.AspNetCore.Mvc;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using System;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
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
