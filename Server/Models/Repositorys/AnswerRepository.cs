using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Server2.Models;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Models.Repositorys
{
    public class AnswerRepository : IAnswerRepository
    {
        private string _connectionString = null;

        public AnswerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private void Create(Answer answer)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            db.Execute("INSERT INTO Answer (TextAnswer, IdSurvey) VALUES(@TextAnswer, @IdSurvey)", new { answer.TextAnswer, answer.IdSurvey });
        }

        public void Create(AnswerDTO answerDTO)
        {
            Create(ConvertAnswerDTOToAnswer(answerDTO, false));
        }

        public AnswerDTO GetAnswerById(int id)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var answer = db.Query<Answer>("SELECT * FROM Answer WHERE Answer.Id = @id", new { id }).FirstOrDefault();
            return ConvertAnswerToAnswerDTO(answer);
        }

        public List<AnswerDTO> GetAnswersBySurvey(Survey survey)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var answers = db.Query<Answer>("SELECT * FROM Answer WHERE IdSurvey = @Id", new { survey.Id }).ToList();

            var answersDTO = new List<AnswerDTO>();
            foreach(var answer in answers)
            {
                answersDTO.Add(ConvertAnswerToAnswerDTO(answer));
            }
            return answersDTO;
        }

        public List<AnswerDTO> GetAnswersBySurveyId(int surveyId)
        {
            SurveyRepository surveyRepository = new SurveyRepository(_connectionString);
            return GetAnswersBySurvey(surveyRepository.ConvertSurveyDTOToSurvey(surveyRepository.GetSurveyBySurveyId(surveyId)));
        }

        /// <summary>
        /// Возвращает все ответы, которые ставил юзер в конкретном опросе(по тексту опроса)
        /// </summary>
        /// <param name="answer">The answer.</param>
        /// <returns></returns>
        public List<AnswerDTO> GetAllAnswersByAnswer(string answer)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            SurveyRepository surveyRepository = new SurveyRepository(_connectionString);
            var answers = db.Query<Answer>("SELECT * FROM Answer  WHERE TextAnswer = @answer", new { answer }).ToList();
            foreach(var Answer in answers)
            {
                Answer.Survey = surveyRepository.ConvertSurveyDTOToSurvey(surveyRepository.GetSurveyBySurveyId(Answer.IdSurvey));
            }
            var answersDTO = new List<AnswerDTO>();
            foreach(var answerInAnswers in answers)
            {
                answersDTO.Add(ConvertAnswerToAnswerDTO(answerInAnswers));
            }
            return answersDTO;
        }

        public List<AnswerDTO> GetAllAnswers()
        {
            IDbConnection db = new SqlConnection(_connectionString);
            SurveyRepository surveyRepository = new SurveyRepository(_connectionString);
            var answers = db.Query<Answer>("SELECT * FROM Answer").ToList();
            foreach(var answer in answers)
            {
                answer.Survey = surveyRepository.ConvertSurveyDTOToSurvey(surveyRepository.GetSurveyBySurveyId(answer.IdSurvey));
            }
            var answersDTO = new List<AnswerDTO>();
            foreach(var answer in answers)
            {
                answersDTO.Add(ConvertAnswerToAnswerDTO(answer));
            }
            return answersDTO;
        }

        public Answer ConvertAnswerDTOToAnswer(AnswerDTO answerDTO, bool convertSurvey = true)
        {
            SurveyRepository surveyRepository = new SurveyRepository(_connectionString);
            VoteRepository voteRepository = new VoteRepository(_connectionString);
            var votes = new List<Vote>();
            if(answerDTO.Votes != null)
            {
                foreach(var voteDTO in answerDTO.Votes)
                {
                    votes.Add(voteRepository.ConvertVoteDTOToVote(voteDTO));
                }
            }
            return new Answer
            {
                Id = (int)answerDTO.Id,
                IdSurvey = (int)answerDTO.IdSurvey,
                Survey = convertSurvey ? surveyRepository.ConvertSurveyDTOToSurvey(surveyRepository.GetSurveyBySurveyId((int)answerDTO.IdSurvey)) : null,
                TextAnswer = answerDTO.TextAnswer,
                Votes = votes.ToArray()
            };
        }

        public AnswerDTO ConvertAnswerToAnswerDTO(Answer answer)
        {
            VoteRepository voteRepository = new VoteRepository(_connectionString);
            var votesDTO = new List<VoteDTO>();
            if(answer.Votes != null)
            {
                foreach(var vote in answer.Votes)
                {
                    votesDTO.Add(voteRepository.ConvertVoteToVoteDTO(vote));
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
