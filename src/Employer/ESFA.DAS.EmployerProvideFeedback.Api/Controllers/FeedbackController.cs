namespace ESFA.DAS.EmployerProvideFeedback.Api.Controllers
{
    using ESFA.DAS.EmployerProvideFeedback.Api.Models;
    using ESFA.DAS.EmployerProvideFeedback.Api.Queries.FeedbackQuery;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : Controller
    {
        private readonly ILogger<FeedbackController> _logger;
        private readonly IMediator _mediator;

        public FeedbackController(IMediator mediator, ILogger<FeedbackController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<EmployerFeedbackDto>))]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAll()
        {
            try
            {

                var result = await _mediator.Send(new FeedbackQuery());
                if (result.Any())
                {
                    return this.Ok(result);
                }

                _logger.LogWarning($"Provider Feedback records database seems to be empty, could not return results");
                return this.NoContent();

            }
            catch (Exception e)
            {
                var message = $"Exception when attempting to get all Provider Feedback records. Message: {e.Message}";
                _logger.LogError(message);
                return this.StatusCode(500, message);
            }
        }
    }
}