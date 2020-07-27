using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Filters
{
    public class AuthorizationFilter : Attribute, IAuthorizationFilter
    {
        private readonly IUserRepository _userRepository;

        public AuthorizationFilter(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if(context.HttpContext.Request.Headers.TryGetValue("AuthorizationToken", out var Token))
            {
                var user = _userRepository.GetUserByToken(Token);
                if(user != null)
                {
                    context.HttpContext.User.AddIdentity(new ClaimsIdentity(user.Id.ToString()));
                }
            }
        }
    }
}
