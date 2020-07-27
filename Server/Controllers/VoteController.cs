using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Server2.Models;
using WebApiQandA.Models.Interfaces;

namespace Server2.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class VoteController : ControllerBase
    {
        public VoteController(IVoteRepository voteRepository) => VoteRepository = voteRepository;

        public IVoteRepository VoteRepository { private set; get; }
        // GET: api/Vote
        [HttpGet]
        public IEnumerable<Vote> Get() => VoteRepository.GetVotes(Request.Headers["Authorization"]);

        // GET: api/Vote/5
        [HttpGet("{id}")]
        public Vote Get(int id) => VoteRepository.Get(Request.Headers["Authorization"], id);

        // GET: api/Vote/getvote
        [HttpPost("getvote")]
        public IEnumerable<Answer> Post([FromBody] Answer[] answer) => VoteRepository.Get(Request.Headers["Authorization"], answer);

        // POST: api/Vote
        [HttpPost]
        public string Post([FromBody] Vote vote) => VoteRepository.Create(Request.Headers["Authorization"], vote);

        // PUT: api/Vote/5
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
