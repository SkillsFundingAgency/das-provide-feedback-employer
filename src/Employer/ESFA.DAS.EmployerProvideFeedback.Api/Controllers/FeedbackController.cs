namespace ESFA.DAS.EmployerProvideFeedback.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ESFA.DAS.EmployerProvideFeedback.Api.Dto;
    using ESFA.DAS.EmployerProvideFeedback.Api.Repository;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Documents.SystemFunctions;
    using Microsoft.Extensions.Logging;

    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : Controller
    {
        private readonly IEmployerFeedbackRepository feedback;
        private readonly ILogger<FeedbackController> logger;

        public FeedbackController(IEmployerFeedbackRepository feedback, ILogger<FeedbackController> logger)
        {
            this.feedback = feedback;
            this.logger = logger;
        }

        [HttpGet, Authorize]
        [ProducesResponseType(200, Type = typeof(List<EmployerFeedback>))]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await this.feedback.GetAllItemsAsync();
                if (result.Any())
                {
                    return this.Ok(result);
                }

                this.logger.LogWarning($"Provider Feedback records database seems to be empty, could not return results");
                return this.NoContent();

            }
            catch (Exception e)
            {
                var message = $"Exception when attempting to get all Provider Feedback records. Message: {e.Message}";
                this.logger.LogError(message);
                return this.StatusCode(500, message);
            }
        }

        [HttpGet("{year}/{month}/{day}", Name = "GetNewer"), Authorize]
        [ProducesResponseType(200, Type = typeof(List<EmployerFeedback>))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetNewer(int year, int month, int day)
        {
            DateTime date = DateTime.UtcNow;
            try
            {
                date = new DateTime(year, month, day, 0, 0, 0);
                var result = await this.feedback.GetItemsAsync(f => f.DateTimeCompleted.CompareTo(date) >= 0);
                if (!result.IsNull())
                {
                    return this.Ok(result);
                }

                this.logger.LogWarning($"Provider Feedback records newer than {date} were not found.");
                return this.NotFound();

            }
            catch (Exception e)
            {
                var message = $"Exception when attempting to get all Provider Feedback records newer than {date.ToLongDateString()}. Message: {e.Message}";
                this.logger.LogError(message);
                return this.StatusCode(500, message);
            }
        }

        [HttpGet("{id}", Name = "GetById"), Authorize]
        [ProducesResponseType(200, Type = typeof(EmployerFeedback))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                EmployerFeedback item = await this.feedback.GetItemAsync(i => i.Id == id);
                if (item != null)
                {
                    return this.Ok(item);
                }

                this.logger.LogWarning($"Provider Feedback record with id {id} was not found.");
                return this.NotFound();

            }
            catch (Exception e)
            {
                var message = $"Exception when attempting to a Provider Feedback record with id {id}. Message: {e.Message}";
                this.logger.LogError(message);
                return this.StatusCode(500, message);
            }
        }
    }
}