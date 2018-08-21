using System;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmitted : ActionFilterAttribute
    {
        private readonly IStoreEmailDetails _emailDetailStore;

        public EnsureFeedbackNotSubmitted(IStoreEmailDetails emailDetailStore)
        {
            _emailDetailStore = emailDetailStore;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var uniqueCode = (Guid)context.ActionArguments["uniqueCode"];
            if (_emailDetailStore.IsFeedbackSubmittedFor(uniqueCode))
            {
                var controller = context.Controller as Controller;
                context.Result = controller.RedirectToRoute(RouteNames.FeedbackAlreadySubmitted);
            }
        }
    }
}
