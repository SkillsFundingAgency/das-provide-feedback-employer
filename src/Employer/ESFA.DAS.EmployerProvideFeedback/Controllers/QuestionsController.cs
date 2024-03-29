﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DAS.EmployerProvideFeedback.Authentication;
using ESFA.DAS.EmployerProvideFeedback.Configuration.Routing;
using ESFA.DAS.EmployerProvideFeedback.Infrastructure;
using ESFA.DAS.EmployerProvideFeedback.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ESFA.DAS.EmployerProvideFeedback.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
    [ServiceFilter(typeof(EnsureFeedbackNotSubmittedRecentlyAttribute))]
    [ServiceFilter(typeof(EnsureSessionExists))]
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
        public async Task<IActionResult> QuestionOne(string encodedAccountId, string returnUrl = null)
        {
            // TODO: Replace TempData by adding a flag to the ViewModel.
            TempData[ReturnUrlKey] = returnUrl;
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var cachedAnswers = await _sessionService.Get<SurveyModel>(idClaim.Value);
            
            // TODO: Redirect from all questions and review route to landing if no survey in the session.

            return View(cachedAnswers);
        }

        [HttpPost("question-one", Name = RouteNames.QuestionOne_Post)]
        public async Task<IActionResult> QuestionOne(SurveyModel surveyModel)
        {
            if (!IsProviderAttributesValid(surveyModel))
            {
                return View(surveyModel);
            }

            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var sessionAnswer = await _sessionService.Get<SurveyModel>(idClaim.Value);
            SetStengths(sessionAnswer, surveyModel.Attributes.Where(x => x.Good));

            await _sessionService.Set(idClaim.Value, sessionAnswer);

            return await HandleRedirect(RouteNames.QuestionTwo_Get);
        }

        private void SetStengths(SurveyModel sessionAnswer, IEnumerable<ProviderAttributeModel> currentAnswerAttributes)
        {
            foreach (var attr in sessionAnswer.Attributes)
            {
                var match = currentAnswerAttributes.SingleOrDefault(x => x.Name == attr.Name);
                attr.Good = match != null;
            }
        }

        private void SetWeaknesses(SurveyModel sessionAnswer, IEnumerable<ProviderAttributeModel> currentAnswerAttributes)
        {
            foreach (var attr in sessionAnswer.Attributes)
            {
                var match = currentAnswerAttributes.SingleOrDefault(x => x.Name == attr.Name);
                attr.Bad = match != null;
            }
        }

        [HttpGet("question-two", Name = RouteNames.QuestionTwo_Get)]
        public async Task<IActionResult> QuestionTwo(string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var sessionAnswers = await _sessionService.Get<SurveyModel>(idClaim.Value);
            return View(sessionAnswers);
        }

        [HttpPost("question-two", Name = RouteNames.QuestionTwo_Post)]
        public async Task<IActionResult> QuestionTwo(SurveyModel surveyModel)
        {
            if (!IsProviderAttributesValid(surveyModel))
            {
                return View(surveyModel);
            }

            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var sessionAnswer = await _sessionService.Get<SurveyModel>(idClaim.Value);

            SetWeaknesses(sessionAnswer, surveyModel.Attributes.Where(x => x.Bad));
            await _sessionService.Set(idClaim.Value, sessionAnswer);
            return await HandleRedirect(RouteNames.QuestionThree_Get);
        }

        

        [HttpGet("question-three", Name = RouteNames.QuestionThree_Get)]
        public async Task<IActionResult> QuestionThree(string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var sessionAnswer = await _sessionService.Get<SurveyModel>(idClaim.Value);
            return View(sessionAnswer);
        }

        [HttpPost("question-three", Name = RouteNames.QuestionThree_Post)]
        public async Task<IActionResult> QuestionThree(SurveyModel surveyModel)
        {
            if (!ModelState.IsValid)
            {
                return View(surveyModel);
            }

            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var sessionAnswer = await _sessionService.Get<SurveyModel>(idClaim.Value);
            sessionAnswer.Rating = surveyModel.Rating;
            await _sessionService.Set(idClaim.Value, sessionAnswer);
            return await HandleRedirect(RouteNames.ReviewAnswers_Get);
        }

        private async Task<IActionResult> HandleRedirect(string nextRoute)
        {
            var returnRoute = Convert.ToString(TempData[ReturnUrlKey]);
            return await Task.Run(() => RedirectToRoute(string.IsNullOrEmpty(returnRoute) ? nextRoute : returnRoute) as IActionResult);
        }

        private bool IsProviderAttributesValid(SurveyModel surveyModel)
        {
            ModelState.TryGetValue(nameof(surveyModel.Attributes), out ModelStateEntry modelState);
            return modelState == null ? true : modelState.ValidationState == ModelValidationState.Valid;
        }
    }
}
