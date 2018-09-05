using System;
using System.Collections.Generic;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class QuestionsController : Controller
    {
        private const string ReturnUrlKey = "ReturnUrl";
        private readonly ISessionService _sessionService;

        public QuestionsController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet("question-one", Name = RouteNames.QuestionOne_Get)]
        public IActionResult QuestionOne(Guid uniqueCode, string returnUrl = null)
        {
            // TODO: Replace TempData by adding a flag to the ViewModel.
            TempData[ReturnUrlKey] = returnUrl;
            var cachedAnswers = _sessionService.Get<SurveyModel>(uniqueCode.ToString());
            
            // TODO: Redirect from all questions and review route to landing if no survey in the session.

            return View(cachedAnswers);
        }

        [HttpPost("question-one", Name = RouteNames.QuestionOne_Post)]
        public IActionResult QuestionOne(Guid uniqueCode, List<ProviderAttributeModel> providerAttributes)
        {
            var sessionAnswer = _sessionService.Get<SurveyModel>(uniqueCode.ToString());
            sessionAnswer.ProviderAttributes = providerAttributes;
            _sessionService.Set(uniqueCode.ToString(), sessionAnswer);

            return HandleRedirect(RouteNames.QuestionTwo_Get);
        }

        [HttpGet("question-two", Name = RouteNames.QuestionTwo_Get)]
        public IActionResult QuestionTwo(Guid uniqueCode, string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var sessionAnswers = _sessionService.Get<SurveyModel>(uniqueCode.ToString());
            return View(sessionAnswers);
        }

        [HttpPost("question-two", Name = RouteNames.QuestionTwo_Post)]
        public IActionResult QuestionTwo(Guid uniqueCode, List<ProviderAttributeModel> providerAttributes)
        {
            var sessionAnswer = _sessionService.Get<SurveyModel>(uniqueCode.ToString());
            sessionAnswer.ProviderAttributes = providerAttributes;
            _sessionService.Set(uniqueCode.ToString(), sessionAnswer);
            return HandleRedirect(RouteNames.QuestionThree_Get);
        }

        [HttpGet("question-three", Name = RouteNames.QuestionThree_Get)]
        public IActionResult QuestionThree(Guid uniqueCode, string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var sessionAnswer = _sessionService.Get<SurveyModel>(uniqueCode.ToString());
            return View(sessionAnswer);
        }

        [HttpPost("question-three", Name = RouteNames.QuestionThree_Post)]
        public IActionResult QuestionThree(Guid uniqueCode, SurveyModel surveyModel)
        {
            if (!ModelState.IsValid)
            {
                return View(surveyModel);
            }

            var sessionAnswer = _sessionService.Get<SurveyModel>(uniqueCode.ToString());
            sessionAnswer.ProviderRating = surveyModel.ProviderRating;
            _sessionService.Set(uniqueCode.ToString(), sessionAnswer);
            return HandleRedirect(RouteNames.ReviewAnswers_Get);
        }

        private IActionResult HandleRedirect(string nextRoute)
        {
            var returnRoute = Convert.ToString(TempData[ReturnUrlKey]);
            return RedirectToRoute(string.IsNullOrEmpty(returnRoute) ? nextRoute : returnRoute);
        }
    }
}
