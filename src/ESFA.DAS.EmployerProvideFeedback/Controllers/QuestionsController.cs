using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    public class QuestionsController : Controller
    {
        [HttpGet("question-one", Name=RouteNames.QuestionOne_Get)]
        public IActionResult QuestionOne()
        {
            return View();
        }

        [HttpGet("question-two", Name=RouteNames.QuestionTwo_Get)]
        public IActionResult QuestionTwo()
        {
            return View();
        }

        [HttpGet("question-three", Name=RouteNames.QuestionThree_Get)]
        public IActionResult QuestionThree()
        {
            return View();
        }
    }
}
