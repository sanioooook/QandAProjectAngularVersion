using System;
using System.Linq;
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
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
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
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
            }
        }
        
        // POST: api/Vote
        [HttpPost]
        public IActionResult Create([FromBody] VoteDto[] votesDto)
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

                if(votesDto.Length <= 0)
                {
                    throw new ArgumentException("Votes array can't be empty", nameof(votesDto));
                }
                votesDto.Aggregate((prev, next) =>
                        next.IdSurvey == prev.IdSurvey ? next : throw new Exception("IdSurvey must be equals on all votes")
                    );
                foreach(var vote in votesDto)
                {
                    if(vote.IdAnswer == null)
                    {
                        throw new ArgumentException("Answer id can't be null", nameof(vote.IdAnswer));
                    }
                    vote.Voter = user.Login;
                }
                _voteService.Create(votesDto);
                return Ok();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
            }
        }
    }
}
