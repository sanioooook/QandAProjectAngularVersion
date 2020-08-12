﻿using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Server2.Models;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Models.Repositoryes
{
	public class UserRepository : IUserRepository
	{
        private readonly string _connectionString = null;
		public UserRepository(string conn) => _connectionString = conn;

        private static string GenRandomString(string alphabet, int length)
		{
			var rnd = new System.Random();
			var sb = new System.Text.StringBuilder(length - 1);
			for(var i = 0; i < length; i++)
			{
				var position = rnd.Next(0, alphabet.Length - 1);
				sb.Append(alphabet[position]);
			}
			return sb.ToString();
		}
		public bool GetAuthorization(string token)
		{
			IDbConnection db = new SqlConnection(_connectionString);
			return db.Query<User>("SELECT * FROM Customer WHERE Autorization = @token", new {token}).FirstOrDefault() !=
                   null;
		}
		public List<User> GetUsers()
		{
			IDbConnection db = new SqlConnection(_connectionString);
			return db.Query<User>("SELECT * FROM Customer").ToList();
		}

		public User Get(int id)
		{
			IDbConnection db = new SqlConnection(_connectionString);
			return db.Query<User>("SELECT * FROM Customer WHERE Id = @id", new { id }).FirstOrDefault();
		}
		public User Get(string login)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            return db.Query<User>("SELECT * FROM Customer WHERE Login = @login", new { login }).FirstOrDefault();
		}

		public User GetUserByToken(string token)
		{
			IDbConnection db = new SqlConnection(_connectionString);
			return db.Query<User>("SELECT * FROM Customer WHERE Customer.Autorization = @token", new { token }).FirstOrDefault();
		}

		public bool Create(User user)
		{
			if(user.Login.Length <= 30 && user.Password.Length <= 40)
			{
				IDbConnection db = new SqlConnection(_connectionString);
				db.Execute("INSERT INTO Customer (Login, Password) VALUES(@Login, @Password)", user);
				return true;
			}
			return false;
		}

        private void Update(User user)
        {
            if(user.Login.Length <= 30 && user.Password.Length <= 40)
            {
                IDbConnection db = new SqlConnection(_connectionString);
                var sqlQuery = "UPDATE Customer SET Login = @Login, Password = @Password WHERE Id = @Id";
                db.Execute(sqlQuery, user);
            }
        }

        public void Update(string login, string password)
        {
            Update(new User { Login = login, Password = password });
        }

        private void Delete(int id)
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            db.Execute("DELETE FROM Customer WHERE Id = @id", new { id });
        }

        public void Delete(string login, string password)
        {
            Delete(GetUsers().LastOrDefault(user => user.Login.Equals(login) && user.Password.Equals(password)).Id);
        }

        public AuthorizeUserDTO Login(string login, string password)
		{
			IDbConnection db = new SqlConnection(_connectionString);
			var resultSelect = db.Query<User>("SELECT * FROM Customer WHERE Login = @login AND Password = @password", new { login, password }).FirstOrDefault();
			if(resultSelect != null)
			{
				resultSelect.Autorization = GenRandomString("QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm", 10);
				db.Execute("UPDATE Customer SET Autorization = @Autorization WHERE Customer.Id = @Id", resultSelect);
			}
			else
            {
				return null;
            }
			return new AuthorizeUserDTO { AuthorizeToken = resultSelect.Autorization};
		}

        public AuthorizeUserDTO Login(User user)
        {
            return Login(user.Login, user.Password);
        }

        public AuthorizeUserDTO Login(UserForLoginOrRegistrationDTO user)
        {
            return Login(user.Login, user.Password);
        }

        public bool Logout(string token)
		{
			var user = GetUserByToken(token);
			if(user is null)
				return false;
			IDbConnection db = new SqlConnection(_connectionString);
			db.Execute("UPDATE Customer SET Autorization = NULL WHERE Customer.Id = @Id", user);
			return true;
		}

	}
}