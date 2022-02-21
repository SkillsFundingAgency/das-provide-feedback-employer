using System;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmitted : ActionFilterAttribute
    {
        private readonly IEmployerFeedbackRepository _employerEmailDetailRepository;

        public EnsureFeedbackNotSubmitted(IEmployerFeedbackRepository employerEmailDetailRepository)
        {
            _employerEmailDetailRepository = employerEmailDetailRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = (StartFeedbackRequest)context.ActionArguments["request"];

            var isCodeBurnt = _employerEmailDetailRepository.IsCodeBurnt(request.UniqueCode).Result;
            if (isCodeBurnt)
            {
                var controller = context.Controller as Controller;
                context.Result = controller.RedirectToRoute(RouteNames.FeedbackAlreadySubmitted);
            }
        }
    }
}
