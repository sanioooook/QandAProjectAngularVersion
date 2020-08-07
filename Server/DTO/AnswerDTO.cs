using System.ComponentModel.DataAnnotations;
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
	}
}
