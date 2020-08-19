using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using WebApiQandA.DTO;
using WebApiQandA.Interfaces;

namespace WebApiQandA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoteController : ControllerBase
    {

        public VoteController(IVoteService voteService, IUserService userService)
        {
            _voteService = voteService;
            _userService = userService;
        }

        private readonly IUserService _userService;
        private readonly IVoteService _voteService;

        // GET: api/Vote
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                Request.Headers.TryGetValue("AuthorizationToken", out var token);
                if(StringValues.IsNullOrEmpty(token))
                {
                    throw new ArgumentException("Token is empty. Please, try again.");
                }
                if(_userService.GetUserByToken(token) == null)
                {
                    throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                return Ok(_voteService.GetAllVotes());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // GET: api/Vote/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                Request.Headers.TryGetValue("AuthorizationToken", out var token);
                if(StringValues.IsNullOrEmpty(token))
                {
                    throw new ArgumentException("Token is empty. Please, try again.");
                }
                if(_userService.GetUserByToken(token) == null)
                {
                    throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                return Ok(_voteService.GetVoteByVoteId(id));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        // POST: api/Vote
        [HttpPost]
        public IActionResult Create([FromBody] VoteDto vote)
        {
            try
            {
                Request.Headers.TryGetValue("AuthorizationToken", out var token);
                if(StringValues.IsNullOrEmpty(token))
                {
                    throw new ArgumentException("Token is empty. Please, try again.");
                }
                var user = _userService.GetUserByToken(token);
                if(user == null)
                {
                    throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                if (vote.IdAnswer == null)
                {
                    throw new ArgumentException("IdAnswer is null");
                }
                vote.Voter = user.Login;
                return Ok(_voteService.Create(vote));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
