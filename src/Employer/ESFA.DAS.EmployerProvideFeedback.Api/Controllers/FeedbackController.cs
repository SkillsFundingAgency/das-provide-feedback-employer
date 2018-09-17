namespace ESFA.DAS.EmployerProvideFeedback.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ESFA.DAS.EmployerProvideFeedback.Api.Models;
    using ESFA.DAS.EmployerProvideFeedback.Api.Repository;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Documents.SystemFunctions;

    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : Controller
    {
        private readonly IEmployerFeedbackRepository feedback;
        private readonly EmployerFeedbackTestHelper testHelper;

        public FeedbackController(IEmployerFeedbackRepository feedback)
        {
            this.feedback = feedback;
        }

        [HttpGet, Authorize]
        [ProducesResponseType(200, Type = typeof(List<EmployerFeedbackDto>))]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await this.feedback.GetAllItemsAsync<EmployerFeedbackDto>();
                if (!result.Any())
                {
                    return this.NoContent();
                }

                return this.Ok(result);
            }
            catch (Exception e)
            {
                return this.StatusCode(500, e.Message);
            }
        }

        [HttpGet("{date}", Name = "GetNewer"), Authorize]
        [ProducesResponseType(200, Type = typeof(List<EmployerFeedbackDto>))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetNewer(DateTime date)
        {
            try
            {
                var result = await this.feedback.GetItemsAsync<EmployerFeedbackDto>(f => f.DateTimeCompleted >= date);
                if (result.IsNull())
                {
                    return this.NotFound();
                }

                return this.Ok(result);
            }
            catch (Exception e)
            {
                return this.StatusCode(500, e.Message);
            }
        }

        [HttpGet("{id}", Name = "GetById"), Authorize]
        [ProducesResponseType(200, Type = typeof(EmployerFeedbackDto))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                EmployerFeedbackDto item = await this.feedback.GetItemAsync<EmployerFeedbackDto>(i => i.Id == id);

                if (item == null)
                {
                    return this.NotFound();
                }

                return this.Ok(item);
            }
            catch (Exception e)
            {
                return this.StatusCode(500, e.Message);
            }
        }
    }
}