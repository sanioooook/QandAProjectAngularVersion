using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Server2.Models;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace Server2.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        public AnswerController(IAnswerRepository answerRepository, IUserRepository userRepository)
        {
            _answerRepository = answerRepository;
            _userRepository = userRepository;
        }

        private IAnswerRepository _answerRepository;

        private IUserRepository _userRepository;

        // GET: api/Answer
/*        [HttpGet]
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
            return Ok(_answerRepository.GetAllAnswers());
        }

        // GET: api/Answer/5
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
            return Ok(_answerRepository.GetAnswerById(id));
        }*/

        // POST: api/Answer
        [HttpPost]
        public IActionResult Post([FromBody] AnswerDTO answer)
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
            _answerRepository.Create(answer);
            return Ok();
        }
    }
}
