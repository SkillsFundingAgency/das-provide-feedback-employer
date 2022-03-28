using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Authorize]
    public class FeedbackSubmittedController : Controller
    {
        [HttpGet("/{encodedAccountId}/feedback-submitted", Name = RouteNames.FeedbackAlreadySubmitted)]
        public IActionResult Index(string encodedAccountId)
        {
            return View();
        }
    }
}