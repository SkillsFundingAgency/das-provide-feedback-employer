using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    public class FeedbackSubmittedController : Controller
    {
        [HttpGet("feedback-submitted", Name = RouteNames.FeedbackAlreadySubmitted)]
        public IActionResult Index()
        {
            return View();
        }
    }
}