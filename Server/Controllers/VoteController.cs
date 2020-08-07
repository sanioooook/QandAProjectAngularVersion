using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace Server2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoteController : ControllerBase
    {

        public VoteController(IVoteRepository voteRepository, IUserRepository userRepository)
        {
            _voteRepository = voteRepository;
            _userRepository = userRepository;
        }

        private IUserRepository _userRepository;

        private IVoteRepository _voteRepository;

        // GET: api/Vote
        [HttpGet]
        public IActionResult Get()
        {
            Request.Headers.TryGetValue("AuthorizationToken", out var token);
            if(StringValues.IsNullOrEmpty(token))
            {
                return BadRequest("Token is empty. Please, try again.");
            }
            if(_userRepository.GetUserByToken(token) == null)
            {
                return BadRequest("Token is incorrect. Please, logout, login and try again");
            }
            return Ok(_voteRepository.GetAllVotes());
        }

        // GET: api/Vote/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Request.Headers.TryGetValue("AuthorizationToken", out var token);
            if(StringValues.IsNullOrEmpty(token))
            {
                return BadRequest("Token is empty. Please, try again.");
            }
            if(_userRepository.GetUserByToken(token) == null)
            {
                return BadRequest("Token is incorrect. Please, logout, login and try again");
            }
            return Ok(_voteRepository.GetVoteById(id));
        }
        /*
                // GET: api/Vote/getvote
                [HttpPost("getvote")]
                public IActionResult Post([FromBody] AnswerDTO[] answer)
                {
                    Request.Headers.TryGetValue("AuthorizationToken", out var token);
                    if(StringValues.IsNullOrEmpty(token))
                    {
                        return BadRequest("Token is empty. Please, try again.");
                    }
                    if(_userRepository.GetUserByToken(token) == null)
                    {
                        return BadRequest("Token is incorrect. Please, logout, login and try again");
                    }
                    return Ok(_voteRepository.FillVotesInAnswers(answer));
                }*/

        // POST: api/Vote
        [HttpPost]
        public IActionResult Post([FromBody] VoteDTO vote)
        {
            Request.Headers.TryGetValue("AuthorizationToken", out var token);
            if(StringValues.IsNullOrEmpty(token))
            {
                return BadRequest("Token is empty. Please, try again.");
            }
            if(_userRepository.GetUserByToken(token) == null)
            {
                return BadRequest("Token is incorrect. Please, logout, login and try again");
            }
            return _voteRepository.Create(vote) ? Ok() : (IActionResult)BadRequest("Error on create, please, try again");
        }
    }
}
