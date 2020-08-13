using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Controllers
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

        private readonly IAnswerRepository _answerRepository;

        private readonly IUserRepository _userRepository;

        // POST: api/Answer
        [HttpPost]
        public IActionResult AddNewAnswer([FromBody] AnswerDTO answer)
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
            return Ok(_answerRepository.Create(answer));
        }

        // DELETE: api/Answer/5
        [HttpDelete("{id}")]
        public IActionResult DeleteAnswer([FromRoute]int id)
        {
            Request.Headers.TryGetValue("AuthorizationToken", out var token);
            if(StringValues.IsNullOrEmpty(token))
            {
                return BadRequest("Token is empty. Please, try again.");
            }
            var user = _userRepository.GetUserByToken(token);
            if(user == null)
            {
                return BadRequest("Token is incorrect. Please, logout, login and try again");
            }
            _answerRepository.DeleteAnswerByAnswerId(user, id);
            return Ok();
        }

        // POST: api/Answer/EditAnswer
        [HttpPost("EditAnswer")]
        public IActionResult EditAnswer([FromBody] AnswerDTO answerDto)
        {
            Request.Headers.TryGetValue("AuthorizationToken", out var token);
            if(StringValues.IsNullOrEmpty(token))
            {
                return BadRequest("Token is empty. Please, try again.");
            }
            var user = _userRepository.GetUserByToken(token);
            if(user == null)
            {
                return BadRequest("Token is incorrect. Please, logout, login and try again");
            }
            if (_answerRepository.GetAnswerById((int)answerDto.Id).TextAnswer == answerDto.TextAnswer)
            {
                return Ok("Don't need edit");
            }
            _answerRepository.EditAnswer(user, answerDto);

            return Ok();
        }
    }
}
