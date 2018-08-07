using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class QuestionsController : Controller
    {
        [HttpGet("question-one", Name=RouteNames.QuestionOne_Get)]
        public IActionResult QuestionOne()
        {
            return View();
        }

        [HttpPost("question-one", Name=RouteNames.QuestionOne_Post)]
        public IActionResult QuestionOne(object answer)
        {
            return RedirectToRoute(RouteNames.QuestionTwo_Get);
        }

        [HttpGet("question-two", Name=RouteNames.QuestionTwo_Get)]
        public IActionResult QuestionTwo()
        {
            return View();
        }
        
        [HttpPost("question-two", Name=RouteNames.QuestionTwo_Post)]
        public IActionResult QuestionTwo(object answer)
        {
            return RedirectToRoute(RouteNames.QuestionThree_Get);
        }

        [HttpGet("question-three", Name=RouteNames.QuestionThree_Get)]
        public IActionResult QuestionThree()
        {
            return View();
        }

        [HttpPost("question-three", Name=RouteNames.QuestionThree_Post)]
        public IActionResult QuestionThree(object answer)
        {
            return RedirectToRoute(RouteNames.ReviewAnswers_Get);
        }
    }
}
