using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Authorize]
    public class ProviderController : Controller
    {
        private readonly ILogger<ProviderController> _logger;

        public ProviderController(
            ILogger<ProviderController> logger
            )
        {
            _logger = logger;
        }


        // Use https://localhost:5030/MRLPWP/providers
        [HttpGet]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
