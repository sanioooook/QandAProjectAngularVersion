using System.Collections.Generic;
using Entities.Models;

namespace Entities.Interfaces
{
    public interface IUserRepository
    {
        List<User> GetUsers();

        User GetUserByUserId(int id);

        User GetUserByLogin(string login);
        
		User GetUserByToken(string token);

        User CreateUser(User user);

        void UpdateUser(User user);

        void DeleteUser(User user);

        string Login(User user);

        void Logout(User user);
    }
}
