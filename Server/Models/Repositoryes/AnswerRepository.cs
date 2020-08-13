using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Server2.Models;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Models.Repositoryes
{
    public class AnswerRepository : IAnswerRepository
    {
        private string _connectionString = null;

        public AnswerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private int Create(Answer answer)
        {
            IDbConnection db = new SqlConnection(_connectionString);

            var returnAnswer = db.Query<Answer>("INSERT INTO Answer (TextAnswer, IdSurvey) VALUES(@TextAnswer, @IdSurvey)",
                new { answer.TextAnswer, answer.IdSurvey }).FirstOrDefault();
            return returnAnswer?.Id ?? 0;
        }

        public int Create(AnswerDTO answerDTO)
        {
            return Create(ConvertAnswerDTOToAnswer(answerDTO, false));
        }

        public AnswerDTO GetAnswerById(int answerId)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var answer = db.Query<Answer>("SELECT * FROM Answer WHERE Answer.Id = @answerId", new { answerId }).FirstOrDefault();
            return ConvertAnswerToAnswerDTO(answer);
        }

        public List<AnswerDTO> GetAnswersBySurvey(Survey survey)
        {
            return GetAnswersBySurveyId(survey.Id);
        }

        public List<AnswerDTO> GetAnswersBySurveyId(int surveyId)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var answers = db.Query<Answer>("SELECT * FROM Answer WHERE IdSurvey = @surveyId", new { surveyId });

            return answers.Select(ConvertAnswerToAnswerDTO).ToList();
        }

        public List<AnswerDTO> GetAllAnswersByTextAnswer(string answer)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var surveyRepository = new SurveyRepository(_connectionString);
            var answers = db.Query<Answer>("SELECT * FROM Answer  WHERE TextAnswer = @answer", new { answer }).ToList();
            foreach(var Answer in answers)
            {
                Answer.Survey = surveyRepository.ConvertSurveyDTOToSurvey(surveyRepository.GetSurveyBySurveyId(Answer.IdSurvey));
            }
            var answersDto = new List<AnswerDTO>();
            foreach(var answerInAnswers in answers)
            {
                answersDto.Add(ConvertAnswerToAnswerDTO(answerInAnswers));
            }
            return answersDto;
        }

        public List<AnswerDTO> GetAllAnswers()
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var surveyRepository = new SurveyRepository(_connectionString);
            var answers = db.Query<Answer>("SELECT * FROM Answer").ToList();
            foreach(var answer in answers)
            {
                answer.Survey = surveyRepository.ConvertSurveyDTOToSurvey(surveyRepository.GetSurveyBySurveyId(answer.IdSurvey));
            }
            var answersDto = new List<AnswerDTO>();
            foreach(var answer in answers)
            {
                answersDto.Add(ConvertAnswerToAnswerDTO(answer));
            }
            return answersDto;
        }

        public void EditAnswer(User user, AnswerDTO answerDto)
        {
            if (GetAnswerById((int) answerDto.Id) != null)
            {
                var surveyRepository = new SurveyRepository(_connectionString);
                if (surveyRepository.GetSurveyBySurveyId((int)answerDto.IdSurvey)?.User.Login == user.Login)
                {
                    IDbConnection db = new SqlConnection(_connectionString);
                    db.Query<Answer>("UPDATE Answer SET TextAnswer = @TextAnswer WHERE Id = @Id", answerDto);
                }
            }
        }

        public void DeleteAnswerByAnswerId(User user, int answerId)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var surveyRepository = new SurveyRepository(_connectionString);
            var idSurvey = GetAnswerById(answerId)?.IdSurvey;
            if (idSurvey != null && surveyRepository.GetSurveyBySurveyId((int) idSurvey)?.User.Login == user.Login)
            {
                var voteRepository = new VoteRepository(_connectionString);
                voteRepository.DeleteVotesByAnswerId(answerId);
                db.Execute("DELETE Answer WHERE Answer.Id = @answerId", new { answerId });
            }
        }

        public void DeleteAnswersBySurveyId(int surveyId)
        {
            var answers = GetAnswersBySurveyId(surveyId);
            if(answers.Count > 0)
            {
                IDbConnection db = new SqlConnection(_connectionString);
                var voteRepository = new VoteRepository(_connectionString);
                foreach(var answer in answers)
                {
                    voteRepository.DeleteVotesByAnswerId((int)answer.Id);
                }
                db.Execute("DELETE Answer WHERE Answer.IdSurvey = @surveyId", new { surveyId });
            }
        }

        public Answer ConvertAnswerDTOToAnswer(AnswerDTO answerDto, bool convertSurvey = true)
        {
            var surveyRepository = new SurveyRepository(_connectionString);
            var voteRepository = new VoteRepository(_connectionString);
            var votes = new List<Vote>();
            if(answerDto.Votes != null)
            {
                foreach(var voteDTO in answerDto.Votes)
                {
                    votes.Add(voteRepository.ConvertVoteDtoToVote(voteDTO));
                }
            }
            return new Answer
            {
                Id = (int)answerDto.Id,
                IdSurvey = (int)answerDto.IdSurvey,
                Survey = convertSurvey ? surveyRepository.ConvertSurveyDTOToSurvey(surveyRepository.GetSurveyBySurveyId((int)answerDto.IdSurvey)) : null,
                TextAnswer = answerDto.TextAnswer,
                Votes = votes.ToArray()
            };
        }

        public AnswerDTO ConvertAnswerToAnswerDTO(Answer answer)
        {
            var voteRepository = new VoteRepository(_connectionString);
            var votesDTO = new List<VoteDTO>();
            if(answer.Votes != null)
            {
                foreach(var vote in answer.Votes)
                {
                    votesDTO.Add(voteRepository.ConvertVoteToVoteDto(vote));
                }
            }
            return new AnswerDTO
            {
                Id = answer.Id,
                IdSurvey = answer.IdSurvey,
                TextAnswer = answer.TextAnswer,
                Votes = votesDTO.ToArray(),
            };
        }
    }
}
