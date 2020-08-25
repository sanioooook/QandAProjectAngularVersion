using System.ComponentModel.DataAnnotations;

namespace WebApiQandA.DTO
{
    public class AuthorizeUserDto
    {
        public string AuthorizeToken { get; set; }

        public string Login { get; set; }
    }
}
