using System;
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
    public class SurveyRepository : ISurveyRepository
    {

        public SurveyRepository(string connectionString)
        {
            _connectionString = connectionString;
            _userRepository = new UserRepository(connectionString);
        }

        private readonly IUserRepository _userRepository;

        private string _connectionString { get; set; }

        private bool Create(User user, Survey survey)
        {
            try
            {
                IDbConnection db = new SqlConnection(_connectionString);
                var answerRepository = new AnswerRepository(_connectionString);
                db.Execute("INSERT INTO Survey (Question, IdCreator, AddResponse, SeveralAnswer, TimeCreate)" + 
                           "VALUES(@Question, @Id, @AddResponse, @SeveralAnswer, @TimeCreate)",
                     new { survey.Question, user.Id, survey.AddResponse, survey.SeveralAnswer, survey.TimeCreate });
                foreach(var answer in survey.Answers)
                {
                    answer.Survey = ConvertSurveyDTOToSurvey(GetSurveysByUserAndQuestion(user, survey.Question).FirstOrDefault());
                    answerRepository.Create(answerRepository.ConvertAnswerToAnswerDTO(answer));
                }
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public bool Create(User user, SurveyDTO survey)
        {
            survey.TimeCreate = DateTime.Now;
            return Create(user, ConvertSurveyDTOToSurvey(survey));
        }

        public SurveyDTO GetSurveyBySurveyId(int surveyId)
        {
            var answerRepository = new AnswerRepository(_connectionString);
            IDbConnection db = new SqlConnection(_connectionString);
            var survey = db.Query<Survey>("SELECT * FROM Survey WHERE Id = @surveyId", new { surveyId }).FirstOrDefault();
            if (survey == null)
            {
                return null;
            }
            var answerDtOs = answerRepository.GetAnswersBySurvey(survey);
            var answers = new List<Answer>();
            foreach(var answerDto in answerDtOs)
            {
                answers.Add(answerRepository.ConvertAnswerDTOToAnswer(answerDto, false));
            }
            survey.Answers = answers.ToArray();
            var voteRepository = new VoteRepository(_connectionString);
            survey.Answers = voteRepository.FillVotesInAnswers(survey.Answers).ToArray();
            return ConvertSurveyToSurveyDto(survey);
        }

        /// <summary>
        /// Возвращает опросы по юзеру
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public List<SurveyDTO> GetSurveysByUser(User user)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var answerRepository = new AnswerRepository(_connectionString);
            var voteRepository = new VoteRepository(_connectionString);
            user = _userRepository.Get(user.Login);
            var surveys = db.Query<Survey>("SELECT * FROM Survey WHERE IdCreator = @Id", new { user.Id }).ToList();
            foreach(var survey in surveys)
            {
                var answerDTOs = answerRepository.GetAnswersBySurvey(survey);
                var answers = new List<Answer>();
                foreach(var answerDTO in answerDTOs)
                {
                    answers.Add(answerRepository.ConvertAnswerDTOToAnswer(answerDTO, false));
                }
                survey.Answers = answers.ToArray();
                survey.Answers = voteRepository.FillVotesInAnswers(survey.Answers).ToArray();
            }
            var surveysDTO = new List<SurveyDTO>();
            foreach(var survey in surveys)
            {
                surveysDTO.Add(ConvertSurveyToSurveyDto(survey));
            }
            return surveysDTO;
        }

        /// <summary>
        /// возвращает опросы по юзеру и вопросу
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="Question">The question.</param>
        /// <returns></returns>
        public List<SurveyDTO> GetSurveysByUserAndQuestion(User user, string Question)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var answerRepository = new AnswerRepository(_connectionString);
            var surveys = db.Query<Survey>("SELECT * FROM Survey WHERE Question = @Question", new { Question }).ToList();
            foreach(var survey in surveys)
            {
                var answerDTOs = answerRepository.GetAnswersBySurvey(survey);
                var answers = new List<Answer>();
                foreach(var answerDTO in answerDTOs)
                {
                    answers.Add(answerRepository.ConvertAnswerDTOToAnswer(answerDTO));
                }
                survey.Answers = answers.ToArray();
            }
            var surveysDTO = new List<SurveyDTO>();
            foreach(var survey in surveys)
            {
                surveysDTO.Add(ConvertSurveyToSurveyDto(survey));
            }
            return surveysDTO;
        }

        /// <summary>
        /// Вовзращает опросы
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public List<SurveyDTO> GetAllSurveys()
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var answerRepository = new AnswerRepository(_connectionString);
            var voteRepository = new VoteRepository(_connectionString);
            var surveys = db.Query<Survey>("SELECT * FROM Survey").ToList();
            foreach(var survey in surveys)
            {
                var answerDTOs = answerRepository.GetAnswersBySurvey(survey);
                var answers = new List<Answer>();
                foreach(var answerDTO in answerDTOs)
                {
                    answers.Add(answerRepository.ConvertAnswerDTOToAnswer(answerDTO, false));
                }
                survey.Answers = answers.ToArray();
                survey.Answers = voteRepository.FillVotesInAnswers(survey.Answers).ToArray();
            }

            var surveysDTO = new List<SurveyDTO>();
            foreach(var survey in surveys)
            {
                surveysDTO.Add(ConvertSurveyToSurveyDto(survey));
            }

            return surveysDTO;
        }

        public bool VoteUser(User user, SurveyDTO surveyDto)
        {
            var surveys = GetAllSurveys();
            return (from survey in surveys
                where survey.Id == surveyDto.Id
                from answer in survey.Answers
                from _ in from vote in answer.Votes where vote.Voter == user.Login select new { }
                select _).Any();
        }

        public Survey ConvertSurveyDTOToSurvey(SurveyDTO surveyDTO)
        {
            var answerRepository = new AnswerRepository(_connectionString);
            var answers = new List<Answer>();
            foreach(var answerDTO in surveyDTO.Answers)
            {
                answers.Add(answerRepository.ConvertAnswerDTOToAnswer(answerDTO));
            }
            return new Survey
            {
                AddResponse = surveyDTO.AddResponse,
                SeveralAnswer = surveyDTO.SeveralAnswer,
                Answers = answers.ToArray(),
                Id = (int)surveyDTO.Id,
                IdCreator = _userRepository.Get(surveyDTO.User.Login).Id,
                Question = surveyDTO.Question,
                TimeCreate = surveyDTO.TimeCreate
            };
        }

        private SurveyDTO ConvertSurveyToSurveyDto(Survey survey)
        {
            var userRepository = new UserRepository(_connectionString);
            var answerRepository = new AnswerRepository(_connectionString);
            var answersDTO = new List<AnswerDTO>();
            foreach(var answer in survey.Answers)
            {
                answersDTO.Add(answerRepository.ConvertAnswerToAnswerDTO(answer));
            }
            return new SurveyDTO
            {
                Id = survey.Id,
                AddResponse = survey.AddResponse,
                Answers = answersDTO.ToArray(),
                Question = survey.Question,
                SeveralAnswer = survey.SeveralAnswer,
                User = new UserForPublic { Login = userRepository.Get(survey.IdCreator).Login },
                TimeCreate = survey.TimeCreate
            };
        }

        public void EditSurvey(SurveyDTO surveyDto)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            db.Execute("UPDATE Survey SET Question = @Question, AddResponse = @AddResponse, SeveralAnswer = @SeveralAnswer WHERE Id = @Id", surveyDto);
        }

        public void DeleteSurveyBySurveyId(User deleter, int surveyId)
        {
            if (GetSurveyBySurveyId(surveyId).User.Login == deleter.Login)
            {
                var answerRepository = new AnswerRepository(_connectionString);
                answerRepository.DeleteAnswersBySurveyId(surveyId);
                IDbConnection db = new SqlConnection(_connectionString);
                db.Execute("DELETE Survey WHERE Id = @surveyId", new { surveyId });
            }
        }
    }
}
