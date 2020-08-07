using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Server2.Models;
using WebApiQandA.DTO;
using WebApiQandA.Models.Interfaces;

namespace WebApiQandA.Models.Repositorys
{
    public class SurveyRepository : ISurveyRepository
    {

        public SurveyRepository(string connectionString)
        {
            _connectionString = connectionString;
            _userRepository = new UserRepository(connectionString);
        }

        IUserRepository _userRepository;

        private string _connectionString { get; set; }

        private bool Create(User user, Survey survey)
        {
            try
            {
                survey.God = user;
                IDbConnection db = new SqlConnection(_connectionString);
                AnswerRepository answerRepository = new AnswerRepository(_connectionString);
                db.Execute("INSERT INTO Survey (Question, IdCreator, AddResponse, SeveralAnswer) VALUES(@Question, @Id, @AddResponse, @SeveralAnswer)",
                     new { survey.Question, survey.God.Id, survey.AddResponse, survey.SeveralAnswer });
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
            return Create(user, ConvertSurveyDTOToSurvey(survey));
        }

        public SurveyDTO GetSurveyBySurveyId(int id)
        {
            AnswerRepository answerRepository = new AnswerRepository(_connectionString);
            IDbConnection db = new SqlConnection(_connectionString);
            var survey = db.Query<Survey>("SELECT * FROM Survey WHERE Id = @id", new { id }).FirstOrDefault();
            survey.God = _userRepository.Get(survey.IdCreator);
            var answerDTOs = answerRepository.GetAnswersBySurvey(survey);
            var answers = new List<Answer>();
            foreach(var answerDTO in answerDTOs)
            {
                answers.Add(answerRepository.ConvertAnswerDTOToAnswer(answerDTO, false));
            }
            survey.Answers = answers.ToArray();
            return ConvertSurveyToSurveyDTO(survey);
        }

        /// <summary>
        /// Возвращает опросы по юзеру
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public List<SurveyDTO> GetSurveysByUser(User user)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            AnswerRepository answerRepository = new AnswerRepository(_connectionString);
            VoteRepository voteRepository = new VoteRepository(_connectionString);
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
                surveysDTO.Add(ConvertSurveyToSurveyDTO(survey));
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
            AnswerRepository answerRepository = new AnswerRepository(_connectionString);
            var surveys = db.Query<Survey>("SELECT * FROM Survey WHERE Question = @Question", new { Question }).ToList();
            foreach(var survey in surveys)
            {
                survey.God = _userRepository.Get(survey.IdCreator);
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
                surveysDTO.Add(ConvertSurveyToSurveyDTO(survey));
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
            AnswerRepository answerRepository = new AnswerRepository(_connectionString);
            VoteRepository voteRepository = new VoteRepository(_connectionString);
            var surveys = db.Query<Survey>("SELECT * FROM Survey").ToList();
            foreach(var survey in surveys)
            {
                survey.God = _userRepository.Get(survey.IdCreator);
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
                surveysDTO.Add(ConvertSurveyToSurveyDTO(survey));
            }

            return surveysDTO;
        }

        /// <summary>
        /// голосовал ли юзер в опросе
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="survey">The survey.</param>
        /// <returns></returns>
        public bool VoteUser(User user, SurveyDTO survey)
        {
            var surveys = GetAllSurveys();
            foreach(var Survey in surveys)
                if(Survey.Id == survey.Id)
                    foreach(var answer in Survey.Answers)
                        foreach(var _ in from vote in answer.Votes
                                         where vote.Voter == user.Login
                                         select new { })
                        {
                            return true;
                        }
            return false;
        }

        public Survey ConvertSurveyDTOToSurvey(SurveyDTO surveyDTO)
        {
            AnswerRepository answerRepository = new AnswerRepository(_connectionString);
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
                God = _userRepository.Get(surveyDTO.User.Login),
                Id = (int)surveyDTO.Id,
                IdCreator = _userRepository.Get(surveyDTO.User.Login).Id,
                Question = surveyDTO.Question
            };
        }

        public SurveyDTO ConvertSurveyToSurveyDTO(Survey survey)
        {
            AnswerRepository answerRepository = new AnswerRepository(_connectionString);
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
                User = new UserForPublic { Login = survey.God.Login }
            };
        }
    }
}
