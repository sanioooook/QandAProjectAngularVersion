using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;
using Entities.Interfaces;
using Entities.Models;

namespace Entities.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _db;

        public UserRepository(string connectionString)
        {
            _db = new SqlConnection(connectionString);
        }

        public List<User> GetUsers()
        {
            return _db.Query<User>("SELECT * FROM Customer").ToList();
        }

        public User GetUserByUserId(int id)
        {
            return _db.Query<User>("SELECT * FROM Customer WHERE Id = @id", new {id}).FirstOrDefault();
        }

        public User GetUserByLogin(string login)
        {
            return _db.Query<User>("SELECT * FROM Customer WHERE Login = @login", new {login}).FirstOrDefault();
        }

        public User GetUserByToken(string token)
        {
            return _db.Query<User>("SELECT * FROM Customer WHERE AuthorizationToken = @token", new {token})
                .FirstOrDefault();
        }

        public User CreateUser(User user)
        {
            return _db.QuerySingle<User>(
                "INSERT INTO Customer (Login, Password) OUTPUT INSERTED.* VALUES(@Login, @Password)", user);
        }

        public void UpdateUser(User user)
        {
            if (_db.Execute("UPDATE Customer SET Login = @Login, Password = @Password WHERE Id = @Id", user) == 0)
            {
                throw new Exception("Update user is failed");
            }
        }

        public void DeleteUser(User user)
        {
            if (_db.Execute("DELETE FROM Customer WHERE Id = @Id", user) == 0)
            {
                throw new Exception("Update user is failed");
            }
        }

        public string Login(User user)
        {
            var authorization = GenRandomString();
            if (_db.Execute("UPDATE Customer SET AuthorizationToken = @authorization WHERE Customer.Id = @Id",
                new {authorization, user.Id}) == 0)
            {
                throw new Exception("Login failed");
            }

            return authorization;
        }

        public void Logout(User user)
        {
            if (_db.Execute("UPDATE Customer SET AuthorizationToken = NULL WHERE Id = @Id", user) == 0)
            {
                throw new Exception("LogOut is failed");
            }
        }

        private static string GenRandomString(string alphabet = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm",
            int length = 10)
        {
            var rnd = new Random();
            var sb = new StringBuilder(length - 1);
            for (var i = 0; i < length; i++)
            {
                var position = rnd.Next(0, alphabet.Length - 1);
                sb.Append(alphabet[position]);
            }

            return sb.ToString();
        }
    }
}