using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    public class QuestionsController : Controller
    {
        public IActionResult QuestionOne()
        {
            return View();
        }

        public IActionResult QuestionTwo()
        {
            return View();
        }

        public IActionResult QuestionThree()
        {
            return View();
        }
    }
}
