using System;
using System.Security.Claims;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmittedRecentlyAttribute : ActionFilterAttribute
    {
        private readonly IEmployerFeedbackRepository _employerEmailDetailRepository;
        private readonly ProvideFeedbackEmployerWebConfiguration _config;

        public EnsureFeedbackNotSubmittedRecentlyAttribute(
            IEmployerFeedbackRepository employerEmailDetailRepository
            , ProvideFeedbackEmployerWebConfiguration config
            )
        {
            _employerEmailDetailRepository = employerEmailDetailRepository;
            _config = config;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var c = context.Controller as Controller;

            if(context.ActionArguments.ContainsKey("uniqueCode"))
            {
                var uniqueCode = (Guid)context.ActionArguments["uniqueCode"];

                var isCodeBurnt = _employerEmailDetailRepository.IsCodeBurnt(uniqueCode).Result;
                if (isCodeBurnt)
                {
                    var dateCodeBurnt = _employerEmailDetailRepository.GetCodeBurntDate(uniqueCode).GetAwaiter().GetResult();
                    if (null != dateCodeBurnt && dateCodeBurnt.HasValue)
                    {
                        var daysSinceFeedback = DateTime.Now - dateCodeBurnt.Value;
                        if (daysSinceFeedback.TotalDays > _config.FeedbackWaitPeriodDays)
                        {
                            return;
                        }
                    }
                    var controller = context.Controller as Controller;
                    context.Result = controller.RedirectToRoute(RouteNames.FeedbackAlreadySubmitted);
                }
            }
        }
    }
}
