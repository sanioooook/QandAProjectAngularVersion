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
    public class VoteRepository : IVoteRepository
    {

        private readonly IDbConnection _db;

        public VoteRepository(string connectionString)
        {
            _db = new SqlConnection(connectionString);
        }

        public Vote Create(Vote vote)
        {
            return _db.QuerySingle<Vote>("INSERT INTO Vote (IdAnswer, IdCustomer, DateVote, IdSurvey) " +
                                         "OUTPUT INSERTED.* VALUES(@IdAnswer, @IdCustomer, @date, @IdSurvey)",
                new { vote.IdAnswer, vote.IdCustomer, vote.IdSurvey, date = DateTime.Now });
        }

        public Vote GetVoteByVoteId(int voteId)
        {
            return _db.Query<Vote>("SELECT * FROM Vote WHERE Id = @id", new { voteId }).FirstOrDefault();
        }

        public List<Vote> GetAllVotes()
        {
            return _db.Query<Vote>("SELECT * FROM Vote").ToList();
        }

        public List<Vote> GetVotesByUserId(int userId)
        {
            return _db.Query<Vote>("SELECT * FROM Vote WHERE IdCustomer = @userId", new { userId }).ToList();
        }

        public void DeleteVotesByAnswerId(int answerId)
        {
            _db.Execute("DELETE Vote WHERE IdAnswer = @answerId", new { answerId });
        }

        public List<Vote> GetVotesByAnswerId(int answerId)
        {
            return _db.Query<Vote>("SELECT * FROM Vote WHERE IdAnswer = @answerId", new { answerId }).ToList();
        }
    }
}
