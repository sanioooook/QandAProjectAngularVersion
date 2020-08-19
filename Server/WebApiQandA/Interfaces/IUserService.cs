using System.Collections.Generic;
using Entities.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Interfaces
{
    public interface IUserService
    {
        bool GetAuthorization(string token);

        List<UserForPublic> GetUsers();

        UserForPublic GetUserById(int id);

        User GetUserByLogin(string login);

        User GetUserByToken(string token);

        UserForPublic Create(UserForLoginOrRegistrationDto user);

        void Update(UserForLoginOrRegistrationDto user);

        void Delete(int id);

        string Login(UserForLoginOrRegistrationDto userForLogin);

        void Logout(string token);
    }
}