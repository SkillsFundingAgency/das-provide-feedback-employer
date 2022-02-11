using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using ESFA.DAS.ProvideFeedback.Domain.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    public class ProviderController : Controller
    {
        private readonly ILogger<ProviderController> _logger;

        public ProviderController(
            ILogger<ProviderController> logger
            )
        {
            _logger = logger;
        }

        //[ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
        //[Route(RoutePrefixPaths.FeedbackRoutePath, Name = RouteNames.Landing_Get)]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var employerId = string.Empty;

            var model = new ProviderSearchViewModel()
            {
                SelectedProviderName = string.Empty,
                ProviderNameFilter = new string[] { },

                SelectedFeedbackStatus = string.Empty,
                FeedbackStatusFilter = new string[] { },

                SelectedDateSubmitted = string.Empty,
                DateSubmittedFilter = new string[] { },
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProviderSearchViewModel model)
        {
            return View(model);
        }






        /// API Endpoint(s) needed. Interface this out in a service.
        /// 
        /// GetTrainingProvidersForEmployer(employerId)
        /// returns:
        /// - paged list of training providers inc page size, number of pages, current page
        /// - provider name filter list
        /// - feedback status filter list
        /// - date submitted filter list
        
    }
}
