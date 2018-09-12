using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DAS.EmployerProvideFeedback.Api.Models;
using ESFA.DAS.FeedbackDataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema.Infrastructure;

namespace ESFA.DAS.EmployerProvideFeedback.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : Controller
    {
        private readonly EmployerFeedbackTestContext _context;
        private readonly EmployerFeedbackTestHelper _testHelper;

        public FeedbackController(EmployerFeedbackTestContext context)
        {
            _context = context;
            _testHelper = new EmployerFeedbackTestHelper();

            if (_context.EmployerFeedback.Any()) return;
            for (var i = 0; i < 1000; i++)
            {
                _context.EmployerFeedback.Add(_testHelper.GenerateRandomFeedback());
            }
            _context.SaveChanges();
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<EmployerFeedback>))]
        [ProducesResponseType(204)]
        public ActionResult<List<EmployerFeedback>> GetAll()
        {
            try
            {
                var result = _context.EmployerFeedback.ToList();
                if (!result.Any())
                {
                    return NoContent();
                }

                return result;
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }

        }

        [HttpGet("{date}", Name = "GetNewer")]
        [ProducesResponseType(200, Type = typeof(List<EmployerFeedback>))]
        [ProducesResponseType(204)]
        public ActionResult<List<EmployerFeedback>> GetNewer(DateTime date)
        {
            try
            {
                var result = _context.EmployerFeedback.Where(f => f.DateTimeCompleted >= date).ToList();
                if (!result.Any())
                {
                    return NotFound();
                }

                return result;
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("{id}", Name = "GetById")]
        [ProducesResponseType(200, Type = typeof(EmployerFeedback))]
        [ProducesResponseType(204)]
        public ActionResult<EmployerFeedback> GetById(Guid id)
        {
            try
            {
                var item = _context.EmployerFeedback.Find(id);

                if (item == null)
                {
                    return NotFound();
                }

                return item;
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
    }
}