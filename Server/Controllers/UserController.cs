using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Server2.Models;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace Server2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IUserRepository _userRepository { get; private set; }

        // GET: api/User
        [HttpGet]
        public IActionResult Get()
        {
            Request.Headers.TryGetValue("AuthorizationToken", out var token);
            if(StringValues.IsNullOrEmpty(token))
            {
                return BadRequest("Token is empty. Please, try again.");
            }
            return Ok(new UserForPublic { Login = _userRepository.GetUserByToken(token).Login });
        }

        // POST: api/User/register
        [HttpPost("Regisration")]
        public IActionResult Post([FromBody] UserForLoginOrRegistrationDTO userDTO)
        {
            if(_userRepository.Get(userDTO.Login) == null)
            {
                var user = new User() { Login = userDTO.Login, Password = userDTO.Password };
                if(_userRepository.Create(user))
                {
                    return Ok(_userRepository.Login(user));
                }
            }
            return BadRequest("Registration is failed. Please, try again");
        }

        // POST: api/User/login
        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserForLoginOrRegistrationDTO user)
        {
            var userDTO = _userRepository.Login(user);
            return userDTO is null ? BadRequest("Login or Password wrong. Please, try again") : (IActionResult)Ok(userDTO);
        }

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            Request.Headers.TryGetValue("AuthorizationToken", out var token);
            if(StringValues.IsNullOrEmpty(token))
            {
                return BadRequest("Token is empty. Please, try again.");
            }
            _userRepository.Logout(token);
            return Ok();
        }
    }
}
