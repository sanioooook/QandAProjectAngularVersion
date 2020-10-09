using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Entities.Interfaces;
using Entities.Models;

namespace Entities.Repositories
{
    public class SurveyRepository : ISurveyRepository
    {

        public SurveyRepository(string connectionString)
        {
            _db = new SqlConnection(connectionString);
        }

        private readonly IDbConnection _db;

        public Survey CreateSurvey(Survey survey)
        {
            return _db.QuerySingle<Survey>("INSERT INTO Survey (Question, IdCreator, AddResponse, TimeCreate," +
                                           " AbilityVoteFrom, AbilityVoteTo, MinCountVotes, MaxCountVotes)" +
                       "OUTPUT INSERTED.* VALUES(@Question, @IdCreator, @AddResponse, @TimeCreate," +
                                           " @AbilityVoteFrom, @AbilityVoteTo, @MinCountVotes, @MaxCountVotes)", survey);
        }

        public Survey GetSurveyBySurveyId(int surveyId)
        {
            return _db.Query<Survey>("SELECT * FROM Survey WHERE Id = @surveyId", new { surveyId }).FirstOrDefault();
        }

        public Survey GetSurveyBySurveyIdAndUserId(in int surveyId, in int userId)
        {
            return _db.Query<Survey>("SELECT * FROM Survey WHERE Id = @surveyId AND IdCreator = @userId", new { surveyId, userId }).FirstOrDefault();
        }

        public int GetCountSurveys(string surveyQuestionFilter = null)
        {
            if(surveyQuestionFilter == null)
            {
                return _db.QuerySingle<int>("SELECT COUNT(*) FROM Survey");
            }
            return _db.QuerySingle<int>("SELECT COUNT(*) FROM Survey WHERE Question = @question",
                new { question = surveyQuestionFilter });
        }

        public List<Survey> GetAllSurveys()
        {
            return _db.Query<Survey>(
                "SELECT * FROM Survey").ToList();
        }
        
        public void EditSurvey(Survey survey)
        {
            _db.Execute(
                "UPDATE Survey SET Question = @Question, AddResponse = @AddResponse, AbilityVoteFrom = @AbilityVoteFrom, " +
                "AbilityVoteTo = @AbilityVoteTo, MinCountVotes = @MinCountVotes, MaxCountVotes = @MaxCountVotes WHERE Id = @Id",
                survey);
        }

        public void DeleteSurveyBySurveyId(int userId, int surveyId)
        {
            _db.Execute("DELETE Survey WHERE Id = @surveyId AND IdCreator = @userId", new { surveyId, userId });
        }
    }
}
