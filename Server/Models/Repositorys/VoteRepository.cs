using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Server2.Models;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Models.Repositorys
{
	public class VoteRepository : IVoteRepository
	{
		string connectionString = null;
		public VoteRepository(string connectionString) => this.connectionString = connectionString;
		public string Create(string token, Vote vote)
		{
			try
			{
				UserRepository userRepository = new UserRepository(connectionString);
				if(userRepository.GetAuthorization(token))
				{
					SurveyRepository surveyRepository = new SurveyRepository(connectionString);
					IDbConnection db = new SqlConnection(connectionString);
					Answer answer = db.Query<Answer>("SELECT * FROM Answer WHERE Answer.Id = @IdAnswer", new { vote.IdAnswer }).FirstOrDefault();
					Survey survey = surveyRepository.Get(token, answer.IdSurvey);
					if(survey.SeveralAnswer || !surveyRepository.VoteUser(token, survey, userRepository.Get(vote.Voter.Login)))
					{
						vote.IdCustomer = userRepository.Get(vote.Voter.Login).Id;
						db = new SqlConnection(connectionString);
						var date = DateTime.Now;
						string DateVote = $"{date.Year}{(date.Month < 9 ? "0" + date.Month.ToString() : date.Month.ToString())}{date.Day} {date.Hour}:{date.Minute}:{date.Second}";
						db.Execute("INSERT INTO Vote (IdAnswer, IdCustomer, DateVote) VALUES(@IdAnswer, @IdCustomer, @DateVote)",
							new { vote.IdAnswer, vote.IdCustomer, DateVote });
						return "Success";
					}
					else
						throw new Exception("Нет разрешения на голосование дважды");
				}
				else
					throw new Exception("неверный токен");
			}
			catch
			{
				return "Fail";
			}
		}

		public Vote Get(string token, int id)
		{
			UserRepository userRepository = new UserRepository(connectionString);
			if(userRepository.GetAuthorization(token))
			{
				IDbConnection db = new SqlConnection(connectionString);
				var vote = db.Query<Vote>("SELECT * FROM Vote WHERE Id = @id", new { id }).FirstOrDefault();
				AnswerRepository answerRepository = new AnswerRepository(connectionString);
				vote.Answer = answerRepository.Get(token, vote.IdAnswer);
				vote.Voter = userRepository.Get(vote.IdCustomer);
				return vote;
			}
			return null;
		}

		public List<Vote> GetVotes(string token)
		{
			UserRepository userRepository = new UserRepository(connectionString);
			if(userRepository.GetAuthorization(token))
			{
				IDbConnection db = new SqlConnection(connectionString);
				var votes = db.Query<Vote>("SELECT * FROM Vote").ToList();
				AnswerRepository answerRepository = new AnswerRepository(connectionString);
				foreach(var vote in votes)
				{
					vote.Answer = answerRepository.Get(token, vote.IdAnswer);
					vote.Voter = userRepository.Get(vote.IdCustomer);
				}
				return votes;
			}
			return null;
		}

		public List<Vote> GetUser(string token, int userId)
		{
			UserRepository userRepository = new UserRepository(connectionString);
			if(userId != 0 && userRepository.GetAuthorization(token))
			{
				User user = userRepository.Get(userId);
				if(user != null)
				{
					AnswerRepository answerRepository = new AnswerRepository(connectionString);
					IDbConnection db = new SqlConnection(connectionString);
					var votes = db.Query<Vote>("SELECT * FROM Vote WHERE IdCustomer = @Id", new { user.Id }).ToList();
					foreach(var vote in votes)
					{
						vote.Answer = answerRepository.Get(token, vote.IdAnswer);
						vote.Voter = user;
					}
					return votes;
				}
			}
			return null;
		}
		public List<Vote> Get(string token, User user)
		{
			UserRepository userRepository = new UserRepository(connectionString);
			if(user != null && user.Login != null && userRepository.GetAuthorization(token))
			{
				AnswerRepository answerRepository = new AnswerRepository(connectionString);
				IDbConnection db = new SqlConnection(connectionString);
				user = userRepository.Get(user.Login);
				if(user != null)
				{
					var votes = db.Query<Vote>("SELECT * FROM Vote WHERE IdCustomer = @Id", new { user.Id }).ToList();
					foreach(var vote in votes)
					{
						vote.Answer = answerRepository.Get(token, vote.IdAnswer);
						vote.Voter = user;
					}
					return votes;
				}
			}
			return null;
		}

		public List<Answer> Get(string token, Answer[] answers)
		{
			UserRepository userRepository = new UserRepository(connectionString);
			if(answers != null && answers.Length >= 1 && userRepository.GetAuthorization(token))
			{
				IDbConnection db = new SqlConnection(connectionString);
				foreach(var answer in answers)
				{
					var votes = db.Query<Vote>("SELECT * FROM Vote WHERE IdAnswer = @Id", new { answer.Id }).ToList();
					foreach(var vote in votes)
						vote.Voter = userRepository.Get(vote.IdCustomer);
					answer.Votes = votes.ToArray();
				}
				return answers.ToList();
			}
			return null;
		}
	}
}
