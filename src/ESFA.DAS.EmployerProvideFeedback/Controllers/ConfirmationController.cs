using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    public class ConfirmationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
