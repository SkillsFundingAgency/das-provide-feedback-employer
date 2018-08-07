using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ReviewAnswersController : Controller
    {
        [HttpGet("review-answers", Name=RouteNames.ReviewAnswers_Get)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("review-answers", Name = RouteNames.ReviewAnswers_Post)]
        public IActionResult Index(object answers)
        {
            return RedirectToRoute(RouteNames.Confirmation_Get);
        }
    }
}
