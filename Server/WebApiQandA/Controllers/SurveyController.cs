using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using WebApiQandA.DTO;
using WebApiQandA.Interfaces;

namespace WebApiQandA.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {

        public SurveyController(ISurveyService surveyRepository, IUserService userRepository)
        {
            _surveyRepository = surveyRepository;
            _userRepository = userRepository;
        }

        private readonly IUserService _userRepository;
        private readonly ISurveyService _surveyRepository;

        // GET: api/Survey
        [HttpGet]
        public IActionResult GetAllSurveys()
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
            return Ok(_surveyRepository.GetAllSurveys());
        }

        // GET: api/Survey/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                Request.Headers.TryGetValue("AuthorizationToken", out var token);
                if(StringValues.IsNullOrEmpty(token))
                {
                    throw new ArgumentException("Token is empty. Please, try again.");
                }
                if(_userRepository.GetUserByToken(token) == null)
                {
                    throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                var survey = _surveyRepository.GetSurveyBySurveyId(id);
                if (survey == null)
                {
                    throw new ArgumentException("Not found");
                }
                return Ok(survey);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: api/Survey/Create
        [HttpPost("Create")]
        public IActionResult CreateSurvey([FromBody] SurveyDto surveyDto)
        {
            try
            {
                Request.Headers.TryGetValue("AuthorizationToken", out var token);
                if(StringValues.IsNullOrEmpty(token))
                {
                    throw new ArgumentException("Token is empty. Please, try again.");
                }
                var user = _userRepository.GetUserByToken(token);
                if(user == null)
                {
                    throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                surveyDto.User = new UserForPublic { Login = user.Login };
                _surveyRepository.Create(surveyDto);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: api/Survey/UserSurveys
        [HttpGet("UserSurveys")]
        public IActionResult GetUserSurveys()
        {
            try
            {
                Request.Headers.TryGetValue("AuthorizationToken", out var token);
                if(StringValues.IsNullOrEmpty(token))
                {
                    throw new ArgumentException("Token is empty. Please, try again.");
                }
                var user = _userRepository.GetUserByToken(token);
                if(user == null)
                {
                    throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                return Ok(_surveyRepository.GetSurveysByUser(user.Id));
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: api/Survey/Edit
        [HttpPost("Edit")]
        public IActionResult EditSurvey([FromBody] SurveyDto surveyDto)
        {
            try
            {
                Request.Headers.TryGetValue("AuthorizationToken", out var token);
                if(StringValues.IsNullOrEmpty(token))
                {
                    throw new ArgumentException("Token is empty. Please, try again.");
                }
                var user = _userRepository.GetUserByToken(token);
                if(user == null)
                {
                    throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                if (surveyDto.Id == null)
                {
                    throw new ArgumentException("SurveyId is null");
                }
                if(_surveyRepository.GetSurveyBySurveyId((int)surveyDto.Id).Equals(surveyDto))
                {
                    throw new ArgumentException("The passed object does not differ from the original one");
                }

                if(_surveyRepository.GetSurveyBySurveyId((int)surveyDto.Id).User.Login != user.Login)
                {
                    throw new ArgumentException("You don't have permissive to edit this survey");
                }

                _surveyRepository.EditSurvey(surveyDto);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSurveyBySurveyId([FromRoute] int id)
        {
            try
            {
                Request.Headers.TryGetValue("AuthorizationToken", out var token);
                if(StringValues.IsNullOrEmpty(token))
                {
                    throw new ArgumentException("Token is empty. Please, try again.");
                }
                var user = _userRepository.GetUserByToken(token);
                if(user == null)
                {
                   throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                _surveyRepository.DeleteSurveyBySurveyId(user.Id, id);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
