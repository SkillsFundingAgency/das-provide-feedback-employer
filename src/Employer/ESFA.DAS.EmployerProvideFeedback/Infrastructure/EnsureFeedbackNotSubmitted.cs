using System;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using ESFA.DAS.ProvideFeedback.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.Encoding;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmitted : ActionFilterAttribute
    {
        private readonly IEmployerFeedbackRepository _employerEmailDetailRepository;
        private readonly IEncodingService _encodingService;

        public EnsureFeedbackNotSubmitted(IEmployerFeedbackRepository employerEmailDetailRepository, IEncodingService encodingService)
        {
            _employerEmailDetailRepository = employerEmailDetailRepository;
            _encodingService = encodingService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var uniqueCode = (Guid)context.ActionArguments["uniqueCode"];

            var isCodeBurnt = _employerEmailDetailRepository.IsCodeBurnt(uniqueCode).Result;
            if (isCodeBurnt)
            {
                var employerEmailDetail = _employerEmailDetailRepository.GetEmployerInviteForUniqueCode(uniqueCode).GetAwaiter().GetResult();
                var encodedAccountId = _encodingService.Encode(employerEmailDetail.AccountId, EncodingType.AccountId);
                var controller = context.Controller as Controller;
                context.Result = controller.RedirectToRoute(RouteNames.FeedbackAlreadySubmitted, new { encodedAccountId = encodedAccountId });
            }
        }
    }
}
