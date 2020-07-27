namespace Server2.Models
{
	public class User//пользователь
	{
		public int Id { get; set; }//ид из базы
		public string Login { get; set; }//логин для входа
		public string Password { get; set; }//пароль для входа
		public string Autorization { get; set; }//токен 
	}
}
