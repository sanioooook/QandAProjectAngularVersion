using System;
using System.ComponentModel.DataAnnotations;

namespace WebApiQandA.DTO
{
    public class VoteDto
    {
        [Required]
        public int? Id { get; set; }

        [Required]
        public int? IdAnswer { get; set; }

        public string Voter { get; set; } = null!;

        public DateTime DateVote { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "IdSurvey must be >=1")]
        public int IdSurvey { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is VoteDto temp && (temp.Id == Id
                                             && temp.DateVote == DateVote
                                             && temp.IdAnswer == IdAnswer
                                             && temp.Voter == Voter
                                             && temp.IdSurvey == IdSurvey);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, IdAnswer, Voter, DateVote, IdSurvey);
        }
    }
}
