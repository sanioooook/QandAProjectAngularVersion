using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
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
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IUserRepository _userRepository { get; private set; }

        // GET: api/User
        [HttpGet]
        public IActionResult Get()
        {
            var users = _userRepository.GetUsers();
            List<string> usersForReturn = new List<string>();
            foreach(var user in users)
            {
                usersForReturn.Add(user.Login);
            }
            return Ok(usersForReturn);
        }

        // GET: api/User/5
        [HttpGet]
        public IActionResult Get([FromQuery] int id)
        {
            var user = _userRepository.Get(id);
            return user != null ? Ok(user.Login) : (IActionResult)BadRequest(new HttpError { Error = "Id is incorrect. Please, try again" });
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
            return BadRequest(new HttpError {Error="Registration is failed. Please, try again" });
        }

        // POST: api/User/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserForLoginOrRegistrationDTO user)
        {
            var userDTO = _userRepository.Login(user);
            return userDTO is null ? BadRequest(new HttpError { Error = "Login or Password wrong. Please, try again" }) : (IActionResult)Ok(userDTO);
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            Request.Headers.TryGetValue("AuthorizationToken", out var token);
            if(token == "")
            {
                return BadRequest(new HttpError { Error = "Token is empty. Please, try again." });
            }
            _userRepository.Logout(token);
            return Ok();
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
