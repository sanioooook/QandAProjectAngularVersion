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
    public class VoteRepository : IVoteRepository
    {
        private string _connectionString;

        public VoteRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private bool Create(Vote vote)
        {
            try
            {
                var surveyRepository = new SurveyRepository(_connectionString);
                var answerRepository = new AnswerRepository(_connectionString);
                var userRepository = new UserRepository(_connectionString);
                IDbConnection db = new SqlConnection(_connectionString);
                var surveyDTO = surveyRepository.GetSurveyBySurveyId((int)answerRepository.GetAnswerById(vote.IdAnswer).IdSurvey);
                if(surveyDTO.SeveralAnswer || !surveyRepository.VoteUser(userRepository.Get(vote.IdCustomer), surveyDTO))
                {
                    var date = DateTime.Now;
                    db.Execute("INSERT INTO Vote (IdAnswer, IdCustomer, DateVote) VALUES(@IdAnswer, @IdCustomer, @date)",
                        new { vote.IdAnswer, vote.IdCustomer, date });
                    return true;
                }
                else
                    throw new Exception("Нет разрешения на голосование дважды");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool Create(VoteDTO vote)
        {
            return Create(ConvertVoteDTOToVote(vote));
        }

        public VoteDTO GetVoteById(int id)
        {
            UserRepository userRepository = new UserRepository(_connectionString);
            IDbConnection db = new SqlConnection(_connectionString);
            var vote = db.Query<Vote>("SELECT * FROM Vote WHERE Id = @id", new { id }).FirstOrDefault();
            return ConvertVoteToVoteDTO(vote);
        }

        public List<VoteDTO> GetAllVotes()
        {
            UserRepository userRepository = new UserRepository(_connectionString);
            IDbConnection db = new SqlConnection(_connectionString);
            var votes = db.Query<Vote>("SELECT * FROM Vote").ToList();
            var voteDTOs = new List<VoteDTO>();
            foreach(var vote in votes)
            {
                voteDTOs.Add(ConvertVoteToVoteDTO(vote));
            }
            return voteDTOs;
        }

        public List<VoteDTO> GetVotesByUser(User user)
        {
            IDbConnection db = new SqlConnection(_connectionString);
            var votes = db.Query<Vote>("SELECT * FROM Vote WHERE IdCustomer = @Id", new { user.Id }).ToList();
            var voteDTOs = new List<VoteDTO>();
            foreach(var vote in votes)
            {
                voteDTOs.Add(ConvertVoteToVoteDTO(vote));
            }
            return voteDTOs;
        }

        public List<VoteDTO> GetVotesByUserId(int userId)
        {
            UserRepository userRepository = new UserRepository(_connectionString);
            var user = userRepository.Get(userId);
            return user != null ? GetVotesByUser(user) : null;
        }

        public List<Answer> FillVotesInAnswers(Answer[] answers)
        {
            UserRepository userRepository = new UserRepository(_connectionString);
            IDbConnection db = new SqlConnection(_connectionString);
            foreach(var answer in answers)
            {
                var votes = db.Query<Vote>("SELECT * FROM Vote WHERE IdAnswer = @Id", new { answer.Id }).ToList();
                answer.Votes = votes.ToArray();
            }
            return answers.ToList();
        }

        public Vote ConvertVoteDTOToVote(VoteDTO voteDTO)
        {
            var userRepository = new UserRepository(_connectionString);
            return new Vote
            {
                Id = (int)voteDTO.Id,
                IdAnswer = (int)voteDTO.IdAnswer,
                DateVote = voteDTO.DateVote,
                IdCustomer = userRepository.Get(voteDTO.Voter).Id
            };
        }

        public VoteDTO ConvertVoteToVoteDTO(Vote vote)
        {
            var userRepository = new UserRepository(_connectionString);
            return new VoteDTO
            {
                Id = vote.Id,
                IdAnswer = vote.IdAnswer,
                DateVote = vote.DateVote,
                Voter = userRepository.Get(vote.IdCustomer).Login
            };
        }
    }
}
