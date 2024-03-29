﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackResultQuery;
using ESFA.DAS.EmployerProvideFeedback.Api.Queries.ProviderSummaryStarsQuery;
using Microsoft.AspNetCore.Authorization;


namespace ESFA.DAS.EmployerProvideFeedback.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployerFeedbackResultController : Controller
    {
        private readonly ILogger<EmployerFeedbackResultController> _logger;
        private readonly IMediator _mediator;

        public EmployerFeedbackResultController(IMediator mediator, ILogger<EmployerFeedbackResultController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(EmployerFeedbackResultDto))]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Route("{ukprn}")]
        public async Task<IActionResult> GetEmployerfeedbackResult(long ukprn)
        {
            try
            {
                EmployerFeedbackResultDto result = await _mediator.Send(new FeedbackResultQuery() { Ukprn = ukprn });
                return Ok(result);
            }
            catch (Exception e)
            {
                var message = $"Exception when attempting to get all Employer Feedback records for UKPRN {ukprn}: {e.Message}";
                _logger.LogError(message);
                return this.StatusCode(500, message);
            }
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<EmployerFeedbackStarsSummaryDto>))]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        [Route("reviews")]
        public async Task<IActionResult> GetAllStarsSummary()
        {
            try
            {
                IEnumerable<EmployerFeedbackStarsSummaryDto> result = await _mediator.Send(new ProviderSummaryStarsQuery());
                return Ok(result);
            }
            catch (Exception e)
            {
                var message = $"Exception when attempting to get all StarsSummary records: {e.Message}";
                _logger.LogError(message);
                return this.StatusCode(500, message);
            }
        }
    }
}