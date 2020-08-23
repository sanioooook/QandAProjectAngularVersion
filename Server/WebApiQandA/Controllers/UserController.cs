using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using WebApiQandA.DTO;
using WebApiQandA.Interfaces;

namespace WebApiQandA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        private readonly IUserService _userService;

        // GET: api/User
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
                return Ok(new UserForPublic { Login = _userService.GetUserByToken(token).Login });
            }
            catch(Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
            }
        }

        // POST: api/User/register
        [HttpPost("Registration")]
        public IActionResult Post([FromBody] UserForLoginOrRegistrationDto userDto)
        {
            try
            {
                if(_userService.GetUserByLogin(userDto.Login) == null)
                {
                    throw new ArgumentException("The user is already in the database");
                }

                _userService.Create(userDto);
                return Ok(new AuthorizeUserDto {AuthorizeToken = _userService.Login(userDto)});
            }
            catch(Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
            }
        }

        // POST: api/User/login
        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserForLoginOrRegistrationDto user)
        {
            try
            {
                if(user == null)
                {
                    throw new ArgumentException("userForLogin is null");
                }
                return Ok(new AuthorizeUserDto {AuthorizeToken = _userService.Login(user)});
            }
            catch (Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
            }  
            
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            try
            {
                Request.Headers.TryGetValue("AuthorizationToken", out var token);
                if(StringValues.IsNullOrEmpty(token))
                {
                    throw new ArgumentException("Token is empty. Please, try again.");
                }
                _userService.Logout(token);
                return Ok();
            }
            catch(Exception e)
            {
                ModelState.AddModelError("Errors", e.Message);
                return BadRequest(ModelState);
            }
        }
    }
}
