using System.ComponentModel.DataAnnotations;
using WebApiQandA.Models;

namespace Server2.Models
{
	public class Answer//ответ
	{
		public int Id { set; get; }//ид ответа

		[StringLength(100)]
		public string TextAnswer { set; get; }//текст ответа

		public int IdSurvey { set; get; }//ид опроса, чтобы легче найти было

		public Survey Survey { set; get; }//ссылка на опрос

		public Vote[] Votes { set; get; }
	}
}
