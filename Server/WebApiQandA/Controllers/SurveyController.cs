using System;
using Entities.Enums;
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

        public SurveyController(ISurveyService surveyService, IUserService userService)
        {
            _surveyService = surveyService;
            _userService = userService;
        }

        private readonly IUserService _userService;
        private readonly ISurveyService _surveyService;

        // GET: api/Survey
        [HttpGet]
        public IActionResult GetAllSurveys([FromQuery] Sort<SurveySortBy> sort,
            [FromQuery] Pagination<SurveyDto> pagination, [FromQuery] string filter)
        {
            try
            {
                Request.Headers.TryGetValue("AuthorizationToken", out var token);
                if(StringValues.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is empty. Please, try again.");
                }
                var user = _userService.GetUserByToken(token);
                if(user == null)
                {
                    throw new ArgumentException("Token is incorrect. Please, logout, login and try again", nameof(token));
                }

                var surveys = _surveyService.GetAllSurveys(sort, user, pagination, new Filter() { SearchQuery = filter });
                var surveysCount = _surveyService.GetCountSurveys();
                pagination.Data = surveys;
                pagination.TotalCount = surveysCount;
                pagination.PageCount = surveysCount/pagination.PageSize;
                return Ok(pagination);
            }
            catch(Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
            }
        }

        // GET: api/Survey/5
        [HttpGet("{id}")]
        public IActionResult GetSurveyById(int id)
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
                var survey = _surveyService.GetSurveyBySurveyId(id);
                if (survey == null)
                {
                    throw new ArgumentException("Not found");
                }
                return Ok(survey);
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
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
                var user = _userService.GetUserByToken(token);
                if(user == null)
                {
                    throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                surveyDto.User = new UserForPublic { Login = user.Login };
                _surveyService.Create(surveyDto);
                return Ok();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
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
                var user = _userService.GetUserByToken(token);
                if(user == null)
                {
                    throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                if (surveyDto.Id == null)
                {
                    throw new ArgumentException("SurveyId is null");
                }
                if(_surveyService.GetSurveyBySurveyId((int)surveyDto.Id).Equals(surveyDto))
                {
                    throw new ArgumentException("The passed object does not differ from the original one");
                }

                if(_surveyService.GetSurveyBySurveyId((int)surveyDto.Id).User.Login != user.Login)
                {
                    throw new ArgumentException("You don't have permissive to edit this survey");
                }

                _surveyService.EditSurvey(surveyDto);
                return Ok();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
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
                var user = _userService.GetUserByToken(token);
                if(user == null)
                {
                   throw new ArgumentException("Token is incorrect. Please, logout, login and try again");
                }
                _surveyService.DeleteSurveyBySurveyId(user.Id, id);
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
