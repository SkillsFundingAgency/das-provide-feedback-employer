using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    public class ReviewAnswersController : Controller
    {
        [HttpGet("review-answers", Name=RouteNames.ReviewAnswers_Get)]
        public IActionResult Index()
        {
            return View();
        }
    }
}
