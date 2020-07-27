using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Server2.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Models.Interfaces
{
	public interface IUserRepository
	{
		public bool Create(User user);

		//void Delete(int id);

		public User Get(int id);

		public User Get(string login);

		//User Get(string login, string password);

		public string Login(User user);

		public void Logout(string token);

		public User GetUserByToken(string token);

		public List<User> GetUsers();

		public string Login(UserForLoginOrRegistrationDTO user);

		//void Update(User user);
	}
}
