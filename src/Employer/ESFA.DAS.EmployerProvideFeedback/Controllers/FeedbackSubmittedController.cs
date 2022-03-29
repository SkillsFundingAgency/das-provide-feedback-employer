using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Shared.UI;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Authorize]
    public class FeedbackSubmittedController : Controller
    {
        private readonly UrlBuilder _urlBuilder;

        public FeedbackSubmittedController(UrlBuilder urlBuilder)
        {
            _urlBuilder = urlBuilder;
        }

        [HttpGet("/{encodedAccountId}/feedback-submitted", Name = RouteNames.FeedbackAlreadySubmitted)]
        public IActionResult Index(string encodedAccountId)
        {
            ViewBag.EmployerAccountsHomeUrl = _urlBuilder.AccountsLink("AccountsHome", encodedAccountId);
            return View();
        }
    }
}