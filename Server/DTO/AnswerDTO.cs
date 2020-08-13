#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Server2.Models;

namespace WebApiQandA.DTO
{
    public class AnswerDTO
	{
		[Required]
		public int? Id { set; get; }//ид ответа

		[Required, StringLength(100)]
		public string TextAnswer { set; get; }//текст ответа

		[Required]
		public int? IdSurvey { set; get; }//ид опроса

		public VoteDTO[] Votes { set; get; }

        public override bool Equals(object? obj)
        {
            return obj is AnswerDTO temp && temp.Votes.Length == Votes.Length &&
                   (!temp.Votes.Where((t, i) => !t.Equals(Votes[i])).Any() && (temp.TextAnswer == TextAnswer
                                                                               && temp.IdSurvey == IdSurvey
                                                                               && temp.Id == Id));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, TextAnswer, IdSurvey, Votes);
        }
    }
}
