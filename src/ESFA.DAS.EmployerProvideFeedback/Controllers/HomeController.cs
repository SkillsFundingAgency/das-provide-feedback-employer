using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet(Name=RouteNames.Landing_Get)]
        public IActionResult Index()
        {
            return View();
        }
    }
}
