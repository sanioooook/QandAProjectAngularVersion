using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Server2.Models;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace Server2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public UserController(IUserRepository userRepository) => UserRepository = userRepository;

        public IUserRepository UserRepository { get; private set; }
        // GET: api/User
        [HttpGet]
        public IEnumerable<User> Get() => UserRepository.GetUsers();

        // GET: api/User/5
        [HttpGet]
        public IActionResult Get([FromQuery] int id)
        {
            return UserRepository.Get(id) != null ? Ok() : (IActionResult)BadRequest();
        }

        // POST: api/User/register
        [HttpPost("register")]
        public IActionResult Post([FromBody] UserForLoginOrRegistrationDTO userDTO)
        {
            if(UserRepository.Get(userDTO.Login) == null)
            {
                var user = new User() { Login = userDTO.Login, Password = userDTO.Password };
                if(UserRepository.Create(user))
                {
                    return Ok(UserRepository.Login(user));
                }
            }
            return BadRequest();
        }

        // POST: api/User/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserForLoginOrRegistrationDTO user)
        {
            if(user.Login != "" && user.Password != "" && user.Login != null && user.Password != null)
                return Ok(UserRepository.Login(user));
            return BadRequest();
        }

        [HttpPost("logout")]
        public void Logout()
        {
            if(Request.Headers["Authorization"] != "")
                UserRepository.Logout(Request.Headers["Authorization"]);
        }
        //// PUT: api/User/5
        //[HttpPut("{id}")]//update
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
