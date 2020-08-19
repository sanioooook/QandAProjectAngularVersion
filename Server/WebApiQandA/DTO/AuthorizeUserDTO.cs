using System.ComponentModel.DataAnnotations;

namespace WebApiQandA.DTO
{
    public class AuthorizeUserDto
    {
        [Required]
        public string AuthorizeToken { get; set; }
    }
}
