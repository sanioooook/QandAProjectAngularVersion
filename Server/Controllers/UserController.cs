using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Server2.Models;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        private readonly IUserRepository _userRepository;

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
        [HttpPost("Registration")]
        public IActionResult Post([FromBody] UserForLoginOrRegistrationDTO userDto)
        {
            if(_userRepository.Get(userDto.Login) == null)
            {
                var user = new User() { Login = userDto.Login, Password = userDto.Password };
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
            var userDto = _userRepository.Login(user);
            return userDto is null ? BadRequest("Login or Password wrong. Please, try again") : (IActionResult)Ok(userDto);
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
