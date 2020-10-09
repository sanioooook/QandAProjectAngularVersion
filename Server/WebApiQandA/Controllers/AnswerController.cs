using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using WebApiQandA.DTO;
using WebApiQandA.Interfaces;

namespace WebApiQandA.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        public AnswerController(IAnswerService answerService,
            IUserService userService,
            ISurveyService surveyService)
        {
            _answerService = answerService;
            _userService = userService;
            _surveyService = surveyService;
        }

        private readonly IAnswerService _answerService;
        private readonly ISurveyService _surveyService;
        private readonly IUserService _userService;

        // POST: api/Answer
        [HttpPost]
        public IActionResult AddNewAnswer([FromBody] AnswerDto answer)
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

                if(answer.IdSurvey != null && answer.IdSurvey != 0 &&
                   _surveyService.IsUserVoteInSurvey(_surveyService.GetSurveyBySurveyId((int)answer.IdSurvey), user)
                )
                {
                    throw new Exception("You can't add answer to a survey if you've already voted in the survey");
                }
                return Ok(_answerService.Create(answer));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
            }
        }

        // DELETE: api/Answer/5
        [HttpDelete("{id}")]
        public IActionResult DeleteAnswer([FromRoute]int id)
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

                var idSurvey = _answerService.GetAnswerByAnswerId(id).IdSurvey;
                if (idSurvey == null)
                {
                    throw new ArgumentException("idSurvey is null");
                }

                if (_surveyService.GetSurveyBySurveyId((int) idSurvey).User.Login != user.Login)
                {
                    throw new ArgumentException("You Don't have permissions to delete answer.");
                }
                _answerService.DeleteAnswerByAnswerId(id);
                return Ok();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
            }
        }

        // POST: api/Answer/EditAnswer
        [HttpPost("EditAnswer")]
        public IActionResult EditAnswer([FromBody] AnswerDto answerDto)
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
                if (answerDto.Id == null)
                {
                    throw new ArgumentException("AnswerId is null");
                }
                if (_answerService.GetAnswerByAnswerId((int)answerDto.Id).TextAnswer == answerDto.TextAnswer)
                {
                    throw new ArgumentException("Don't need edit");
                }
                _answerService.EditAnswer(answerDto);

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
