using System.ComponentModel.DataAnnotations;

namespace WebApiQandA.DTO
{
	public class UserForLoginOrRegistrationDTO
	{
		[Required, EmailAddress]
		public string Login { get; set; }

		[Required, StringLength(int.MaxValue, MinimumLength = 8)]
		public string Password { get; set; }
	}
}
