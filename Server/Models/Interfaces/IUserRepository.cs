using System.Collections.Generic;
using Server2.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Models.Interfaces
{
    public interface IUserRepository
	{
		public bool Create(User user);

		public User Get(int id);

		public User Get(string login);

		public AuthorizeUserDTO Login(User user);

		public User GetUserByToken(string token);

		public List<User> GetUsers();

		public AuthorizeUserDTO Login(UserForLoginOrRegistrationDTO user);

		public void Update(string login, string password);

		public bool Logout(string token);

        void Delete(string login, string password);
    }
}
