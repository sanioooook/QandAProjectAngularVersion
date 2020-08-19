using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApiQandA.DTO
{
    public class AnswerDto
	{
		[Required]
		public int? Id { set; get; }

		[Required, StringLength(100)]
		public string TextAnswer { set; get; }

		[Required]
		public int? IdSurvey { set; get; }

		public List<VoteDto> Votes { set; get; }

        public override bool Equals(object? obj)
        {
            return obj is AnswerDto temp && temp.Votes.Count == Votes.Count &&
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
