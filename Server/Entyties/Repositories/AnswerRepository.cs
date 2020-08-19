using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Entities.Interfaces;
using Entities.Models;

namespace Entities.Repositories
{
    public class AnswerRepository : IAnswerRepository
    {

        private readonly IDbConnection _db;
        public AnswerRepository(string connectionString)
        {
            _db = new SqlConnection(connectionString);
        }

        public Answer Create(Answer answer)
        {
            return _db.QuerySingle<Answer>("INSERT INTO Answer (TextAnswer, IdSurvey) OUTPUT INSERTED.* VALUES(@TextAnswer, @IdSurvey)", answer);
        }
        
        public Answer GetAnswerByAnswerId(int answerId)
        {
            return _db.Query<Answer>("SELECT * FROM Answer WHERE Answer.Id = @answerId", new { answerId }).FirstOrDefault();
        }
        
        public List<Answer> GetAnswersBySurveyId(int surveyId)
        {
            return _db.Query<Answer>("SELECT * FROM Answer WHERE IdSurvey = @surveyId", new { surveyId }).ToList();
        }
        
        public List<Answer> GetAllAnswers()
        {
            return _db.Query<Answer>("SELECT * FROM Answer").ToList();
        }

        public void EditAnswer(Answer answer)
        {
            _db.Query<Answer>("UPDATE Answer SET TextAnswer = @TextAnswer WHERE Id = @Id", answer);
        }

        public void DeleteAnswerByAnswerId(int answerId)
        {
            _db.Execute("DELETE Answer WHERE Answer.Id = @answerId", new {answerId});
        }

        public void DeleteAnswersBySurveyId(int surveyId)
        {
            _db.Execute("DELETE Answer WHERE Answer.IdSurvey = @surveyId", new { surveyId });
        }
    }
}
