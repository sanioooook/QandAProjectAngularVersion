using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Server2.Models;
using WebApiQandA.Models.Interfaces;

namespace Server2.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        public AnswerController(IAnswerRepository answerRepository) => AnswerRepository = answerRepository;

        public IAnswerRepository AnswerRepository { get; private set; }
        // GET: api/Answer
        [HttpGet]
        public IEnumerable<Answer> Get() => AnswerRepository.GetAnswers(Request.Headers["Authorization"]);

        // GET: api/Answer/5
        [HttpGet("{id}")]
        public Answer Get(int id) => AnswerRepository.Get(Request.Headers["Authorization"], id);

        // POST: api/Answer
        [HttpPost]
        public string Post([FromBody] Answer answer) => AnswerRepository.Create(Request.Headers["Authorization"], answer);

        // PUT: api/Answer/5
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
