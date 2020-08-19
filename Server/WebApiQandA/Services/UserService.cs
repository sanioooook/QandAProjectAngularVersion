using System;
using System.Collections.Generic;
using Entities.Interfaces;
using Entities.Models;
using WebApiQandA.DTO;
using WebApiQandA.Interfaces;

namespace WebApiQandA.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public bool GetAuthorization(string token)
        {
            return _userRepository.GetUserByToken(token) != null;
        }

        public List<UserForPublic> GetUsers()
        {
            var users = _userRepository.GetUsers();
            var usersForPublic = new List<UserForPublic>();
            users.ForEach(user => usersForPublic.Add(new UserForPublic { Login = user.Login }));
            return usersForPublic;
        }

        public UserForPublic GetUserById(int id)
        {
            return new UserForPublic { Login = _userRepository.GetUserByUserId(id).Login };
        }

        public User GetUserByLogin(string login)
        {
            return _userRepository.GetUserByLogin(login);
        }

        public User GetUserByToken(string token)
        {
            return _userRepository.GetUserByToken(token);
        }

        public UserForPublic Create(UserForLoginOrRegistrationDto user)
        {
            return new UserForPublic
            { Login = _userRepository.CreateUser(new User { Login = user.Login, Password = user.Password }).Login };
        }

        public void Update(UserForLoginOrRegistrationDto user)
        {
            _userRepository.UpdateUser(GetUserByLogin(user.Login));
        }

        public void Delete(int id)
        {
            _userRepository.DeleteUser(_userRepository.GetUserByUserId(id));
        }

        public string Login(UserForLoginOrRegistrationDto userForLogin)
        {
            var user = GetUserByLogin(userForLogin.Login);
            if (user != null && user.Password == userForLogin.Password) 
            {
                return _userRepository.Login(user);
            }
            throw new ArgumentException("Login or password wrong, please, try again letter");
        }

        public void Logout(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(token));
            }
            var user = GetUserByToken(token);
            if(user == null)
            {
                throw new ArgumentException($"the user who has this token ({token}) does not exist");
            }
            _userRepository.Logout(user);
        }
    }
}
