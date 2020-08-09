using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class SurveyController : ControllerBase
    {

        public SurveyController(ISurveyRepository surveyRepository, IUserRepository userRepository)
        {
            _surveyRepository = surveyRepository;
            _userRepository = userRepository;
        }

        private readonly IUserRepository _userRepository;

        private readonly ISurveyRepository _surveyRepository;

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
            Request.Headers.TryGetValue("AuthorizationToken", out var token);
            if(StringValues.IsNullOrEmpty(token))
            {
                return BadRequest("Token is empty. Please, try again.");
            }
            if(_userRepository.GetUserByToken(token) == null)
            {
                return BadRequest("Token is incorrect. Please, logout, login and try again");
            }
            var survey = _surveyRepository.GetSurveyBySurveyId(id);
            if (survey == null)
            {
                return NotFound("Not found");
            }
            return Ok(survey);
        }

        // POST: api/Survey/Create
        [HttpPost("Create")]
        public IActionResult CreateSurvey([FromBody] SurveyDTO surveyDto)
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
            return _surveyRepository.Create(user, surveyDto) ? Ok() : (IActionResult)BadRequest("Error create survey, please, try again");
        }


        // POST: api/Survey/UserSurveys
        [HttpGet("UserSurveys")]
        public IActionResult GetUserSurveys()
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
            return Ok(_surveyRepository.GetSurveysByUser(user));
        }
    }
}
