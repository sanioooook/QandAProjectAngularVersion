using System;
using System.ComponentModel.DataAnnotations;

namespace WebApiQandA.DTO
{
    public class VoteDTO
    {
        [Required]
        public int? Id { get; set; }

        [Required]
        public int? IdAnswer { get; set; }//ид ответа за который отдан голос

        [Required]
        public string Voter { get; set; }//кто голосовал

        public DateTime DateVote { get; set; }//когда голосовал

        public override bool Equals(object? obj)
        {
            return obj is VoteDTO temp && (temp.Id == Id
                                             && temp.DateVote == DateVote
                                             && temp.IdAnswer == IdAnswer
                                             && temp.Voter == Voter);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, IdAnswer, Voter, DateVote);
        }
    }
}
