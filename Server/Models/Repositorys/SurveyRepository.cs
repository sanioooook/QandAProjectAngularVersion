using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Server2.Models;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Models.Repositorys
{
	public class SurveyRepository : ISurveyRepository
	{
		UserRepository userRepository;

		public SurveyRepository(string connectionString)
		{
			ConnectionString = connectionString;
			userRepository = new UserRepository(connectionString);
		}

		public string ConnectionString { get; set; } = null;

		public string Create(string token, Survey survey)
		{
			if(survey.Question.Length <= 100 && survey.God != null && userRepository.GetAuthorization(token))
			{
				survey.God = userRepository.Get(survey.God.Login);
				IDbConnection db = new SqlConnection(ConnectionString);
				AnswerRepository answerRepository = new AnswerRepository(ConnectionString);
				db.Execute("INSERT INTO Survey (Question, IdCreator, AddResponse, SeveralAnswer) VALUES(@Question, @Id, @AddResponse, @SeveralAnswer)"
					, new { survey.Question, survey.God.Id, survey.AddResponse, survey.SeveralAnswer });
				foreach(var answer in survey.Answers)
				{
					answer.Survey = Get(token, survey.Question).FirstOrDefault();
					answerRepository.Create(token, answer);
				}
				return "Success";
			}
			return "Fail";
		}

		public Survey Get(string token, int id)
		{
			if(userRepository.GetAuthorization(token))
			{
				AnswerRepository answerRepository = new AnswerRepository(ConnectionString);
				IDbConnection db = new SqlConnection(ConnectionString);
				var survey = db.Query<Survey>("SELECT * FROM Survey WHERE Id = @id", new { id }).FirstOrDefault();
				survey.God = userRepository.Get(survey.IdCreator);
				survey.Answers = answerRepository.Get(token, survey).ToArray();
				return survey;
			}
			return null;
		}

		public List<Survey> Get(string token, User God)
		{
			if(God.Login != null && userRepository.GetAuthorization(token))
			{
				IDbConnection db = new SqlConnection(ConnectionString);
				AnswerRepository answerRepository = new AnswerRepository(ConnectionString);
				VoteRepository voteRepository = new VoteRepository(ConnectionString);
				God = userRepository.Get(God.Login);
				var surveys = db.Query<Survey>("SELECT * FROM Survey WHERE IdCreator = @Id", new { God.Id }).ToList();
				foreach(var survey in surveys)
				{
					survey.Answers = answerRepository.Get(token, survey).ToArray();
					survey.Answers = voteRepository.Get(token, survey.Answers).ToArray();
				}
				return surveys;
			}
			else
				return null;
		}

		public List<Survey> Get(string token, string Question)
		{
			if(userRepository.GetAuthorization(token))
			{
				IDbConnection db = new SqlConnection(ConnectionString);
				AnswerRepository answerRepository = new AnswerRepository(ConnectionString);
				var surveys = db.Query<Survey>("SELECT * FROM Survey WHERE Question = @Question", new { Question }).ToList();
				foreach(var survey in surveys)
				{
					survey.God = userRepository.Get(survey.IdCreator);
					survey.Answers = answerRepository.Get(token, survey).ToArray();
				}
				return surveys;
			}
			return null;
		}

		public List<Survey> GetSurveys(string token)
		{
			if(userRepository.GetAuthorization(token))
			{
				IDbConnection db = new SqlConnection(ConnectionString);
				AnswerRepository answerRepository = new AnswerRepository(ConnectionString);
				VoteRepository voteRepository = new VoteRepository(ConnectionString);
				var surveys = db.Query<Survey>("SELECT * FROM Survey").ToList();
				foreach(var survey in surveys)
				{
					survey.God = userRepository.Get(survey.IdCreator);
					survey.Answers = answerRepository.Get(token, survey).ToArray();
					survey.Answers = voteRepository.Get(token, survey.Answers).ToArray();
				}
				return surveys;
			}
			return null;
		}

		public bool VoteUser(string token, Survey survey, User user)
		{
			if(userRepository.GetAuthorization(token))
			{
				var surveys = GetSurveys(token);
				foreach(var Survey in surveys)
					if(Survey.Id == survey.Id)
						foreach(var answer in Survey.Answers)
							foreach(var _ in from vote in answer.Votes
											 where vote.IdCustomer == user.Id || vote.Voter == user
											 select new { })
							{
								return true;
							}
			}
			return false;
		}
	}
}
