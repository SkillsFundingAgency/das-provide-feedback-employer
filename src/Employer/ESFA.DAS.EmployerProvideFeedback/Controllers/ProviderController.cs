using ESFA.DAS.EmployerProvideFeedback.ViewModels;
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
        // Account Id 40907
        // Department of Education Levy Payer in TEST.
        // Should be the CI Log in
        [HttpGet]
        [Route("/{encodedAccountId}/providers")]
        public async Task<IActionResult> Index(GetProvidersForFeedbackRequest request)
        {
            var accountId = request.AccountId;
            return View();
        }
    }
}
