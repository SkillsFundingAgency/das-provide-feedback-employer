using Microsoft.AspNetCore.Mvc.Filters;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmitted : ActionFilterAttribute
    {
        public EnsureFeedbackNotSubmitted()
        {

        }

        //public override void OnActionExecuting(ActionExecutingContext context)
        //{
        //    if (_sessionService.LoggedInUser == null)
        //    {
        //        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        //        context.Result = new JsonResult("Unauthorized");
        //    }
        //}
    }
}
