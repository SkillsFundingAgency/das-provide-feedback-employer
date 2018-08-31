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
        private readonly AnswerModel _answerModel;

        public QuestionsController(ISessionService sessionService, IOptions<List<ProviderAttributeModel>> providerAttributes)
        {
            _sessionService = sessionService;
            _answerModel = new AnswerModel { ProviderAttributes = providerAttributes.Value };
        }

        [HttpGet("question-one", Name = RouteNames.QuestionOne_Get)]
        public IActionResult QuestionOne(Guid uniqueCode, string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var cachedAnswers = _sessionService.Get<AnswerModel>(uniqueCode.ToString());
            return View(cachedAnswers != null ? cachedAnswers.ProviderAttributes : _answerModel.ProviderAttributes);
        }

        [HttpPost("question-one", Name = RouteNames.QuestionOne_Post)]
        public IActionResult QuestionOne(Guid uniqueCode, List<ProviderAttributeModel> providerAttributes)
        {
            var sessionAnswer = GetCurrentAnswerModel(uniqueCode);
            sessionAnswer.ProviderAttributes = providerAttributes;
            _sessionService.Set(uniqueCode.ToString(), sessionAnswer);
            return HandleRedirect(RouteNames.QuestionTwo_Get);
        }

        [HttpGet("question-two", Name = RouteNames.QuestionTwo_Get)]
        public IActionResult QuestionTwo(Guid uniqueCode, string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var sessionAnswers = GetCurrentAnswerModel(uniqueCode);
            return View(sessionAnswers.ProviderAttributes);
        }

        [HttpPost("question-two", Name = RouteNames.QuestionTwo_Post)]
        public IActionResult QuestionTwo(Guid uniqueCode, List<ProviderAttributeModel> providerAttributes)
        {
            var sessionAnswer = GetCurrentAnswerModel(uniqueCode);
            sessionAnswer.ProviderAttributes = providerAttributes;
            _sessionService.Set(uniqueCode.ToString(), sessionAnswer);
            return HandleRedirect(RouteNames.QuestionThree_Get);
        }

        [HttpGet("question-three", Name = RouteNames.QuestionThree_Get)]
        public IActionResult QuestionThree(Guid uniqueCode, string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var sessionAnswer = GetCurrentAnswerModel(uniqueCode);
            return View(sessionAnswer);
        }

        [HttpPost("question-three", Name = RouteNames.QuestionThree_Post)]
        public IActionResult QuestionThree(Guid uniqueCode, AnswerModel answerModel)
        {
            if (!ModelState.IsValid)
            {
                return View(answerModel);
            }

            var sessionAnswer = GetCurrentAnswerModel(uniqueCode);
            sessionAnswer.ProviderRating = answerModel.ProviderRating;
            _sessionService.Set(uniqueCode.ToString(), sessionAnswer);
            return HandleRedirect(RouteNames.ReviewAnswers_Get);
        }

        private AnswerModel GetCurrentAnswerModel(Guid uniqueCode)
        {
            return _sessionService.Get<AnswerModel>(uniqueCode.ToString()) ?? _answerModel;
        }

        private IActionResult HandleRedirect(string nextRoute)
        {
            var returnRoute = Convert.ToString(TempData[ReturnUrlKey]);
            return RedirectToRoute(string.IsNullOrEmpty(returnRoute) ? nextRoute : returnRoute);
        }
    }
}
