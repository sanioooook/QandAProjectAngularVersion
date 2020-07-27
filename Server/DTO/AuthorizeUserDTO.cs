using System.ComponentModel.DataAnnotations;

namespace WebApiQandA.DTO
{
    public class AuthorizeUserDTO
    {
        [Required]
        public string AuthorizeToken { get; set; }
    }
}
