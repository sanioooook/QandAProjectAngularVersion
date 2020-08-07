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
    public class SurveyController : ControllerBase
    {

        public SurveyController(ISurveyRepository surveyRepository, IUserRepository userRepository)
        {
            _surveyRepository = surveyRepository;
            _userRepository = userRepository;
        }

        private IUserRepository _userRepository;

        private ISurveyRepository _surveyRepository;

        // GET: api/Survey
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
            return Ok(_surveyRepository.GetAllSurveys());
        }

        // GET: api/Survey/5
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
            return Ok(_surveyRepository.GetSurveyBySurveyId(id));
        }

        // POST: api/Survey/Create
        [HttpPost("Create")]
        public IActionResult Post([FromBody] SurveyDTO surveyDTO)
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
            return _surveyRepository.Create(user, surveyDTO) ? Ok() : (IActionResult)BadRequest("Error create survey, please, try again");
        }


        // POST: api/Survey/UserSurveys
        [HttpPost("UserSurveys")]
        public IActionResult Post()
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
