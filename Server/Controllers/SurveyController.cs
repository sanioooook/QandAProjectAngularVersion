using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Server2.Models;
using WebApiQandA.Models.Interfaces;

namespace Server2.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {
        public SurveyController(ISurveyRepository surveyRepository) => SurveyRepository = surveyRepository;

        public ISurveyRepository SurveyRepository { get; private set; }
        // GET: api/Survey
        [HttpGet]
        public IEnumerable<Survey> Get()
        {
            return SurveyRepository.GetSurveys(Request.Headers["Authorization"]);
        }

        // GET: api/Survey/5
        [HttpGet("{id}")]
        public Survey Get(int id) => SurveyRepository.Get(Request.Headers["Authorization"], id);

        // GET: api/Survey/Question/{Question}
        //[HttpGet("Question/{Question}")]
        public IEnumerable<Survey> Get(string Question) => SurveyRepository.Get(Request.Headers["Authorization"], Question);

        // POST: api/Survey
        [HttpPost]
        public string Post([FromBody] Survey survey) => SurveyRepository.Create(Request.Headers["Authorization"], survey);


        // POST: api/Survey
        [HttpPost("User")]
        public IEnumerable<Survey> Post([FromBody] User user) => SurveyRepository.Get(Request.Headers["Authorization"], user);
        // PUT: api/Survey/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
