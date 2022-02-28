using System;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmittedRecently : ActionFilterAttribute
    {
        private readonly IEmployerFeedbackRepository _employerEmailDetailRepository;
        private readonly ProvideFeedbackEmployerWeb _config;

        public EnsureFeedbackNotSubmittedRecently(
            IEmployerFeedbackRepository employerEmailDetailRepository
            , ProvideFeedbackEmployerWeb config
            )
        {
            _employerEmailDetailRepository = employerEmailDetailRepository;
            _config = config;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var uniqueCode = (Guid)context.ActionArguments["uniqueCode"];

            var isCodeBurnt = _employerEmailDetailRepository.IsCodeBurnt(uniqueCode).Result;
            if (isCodeBurnt)
            {
                var dateCodeBurnt = _employerEmailDetailRepository.GetCodeBurntDate(uniqueCode).GetAwaiter().GetResult();
                if(null != dateCodeBurnt && dateCodeBurnt.HasValue)
                {
                    var daysSinceFeedback = DateTime.Now - dateCodeBurnt.Value;
                    if(daysSinceFeedback.TotalDays > _config.FeedbackWaitPeriodDays)
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
