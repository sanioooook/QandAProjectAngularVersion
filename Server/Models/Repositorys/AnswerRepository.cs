using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Server2.Models;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Models.Repositorys
{
	public class AnswerRepository : IAnswerRepository
	{
		string connectionString = null;
		public AnswerRepository(string connectionString) => this.connectionString = connectionString;

		public string Create(string token, Answer answer)
		{
			if(answer.TextAnswer.Length <= 100 && answer.Survey != null)
			{
				IDbConnection db = new SqlConnection(connectionString);
				if(answer.Survey.Id == 0 && answer.Survey.Question != null)
				{
					SurveyRepository surveyRepository = new SurveyRepository(connectionString);
					answer.Survey = surveyRepository.Get(token, answer.Survey.Question).FirstOrDefault();
				}
				else if(answer.Survey.Id == 0 && answer.Survey.Question == null)
					return "Fail";
				db.Execute("INSERT INTO Answer (TextAnswer, IdSurvey) VALUES(@TextAnswer, @Id)", new { answer.TextAnswer, answer.Survey.Id });
				return "Success";
			}
			return "Fail";
		}

		public Answer Get(string token, int id)
		{
			IDbConnection db = new SqlConnection(connectionString);
			//SurveyRepository surveyRepository = new SurveyRepository(connectionString);
			var answer = db.Query<Answer>("SELECT * FROM Answer WHERE Answer.Id = @id", new { id }).FirstOrDefault();
			//answer.Survey = surveyRepository.Get(answer.IdSurvey);
			return answer;
		}

		public List<Answer> Get(string token, Survey survey)
		{
			IDbConnection db = new SqlConnection(connectionString);
			if(survey != null)
			{
				SurveyRepository surveyRepository = new SurveyRepository(connectionString);
				if(survey.Id == 0 && survey.Question != null)
					survey = surveyRepository.Get(token, survey.Question).FirstOrDefault();
				else if(survey.Id == 0 && survey.Question == null)
					return null;
				var answers = db.Query<Answer>("SELECT * FROM Answer WHERE IdSurvey = @Id", new { survey.Id }).ToList();
				//foreach(var answer in answers)
				//	answer.Survey = survey;
				return answers;
			}
			else
				return null;
		}

		public List<Answer> Get(string token, string answer)
		{
			IDbConnection db = new SqlConnection(connectionString);
			SurveyRepository surveyRepository = new SurveyRepository(connectionString);
			var answers = db.Query<Answer>("SELECT * FROM Answer  WHERE TextAnswer = @answer", new { answer }).ToList();
			foreach(var Answer in answers)
				Answer.Survey = surveyRepository.Get(token, Answer.IdSurvey);
			return answers;
		}

		public List<Answer> GetAnswers(string token)
		{
			IDbConnection db = new SqlConnection(connectionString);
			SurveyRepository surveyRepository = new SurveyRepository(connectionString);
			var answers = db.Query<Answer>("SELECT * FROM Answer ").ToList();
			foreach(var answer in answers)
				answer.Survey = surveyRepository.Get(token, answer.IdSurvey);
			return answers;
		}
	}
}
