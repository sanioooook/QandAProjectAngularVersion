using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
	public class Answer
	{
		public int Id { set; get; }

		[StringLength(100)]
		public string TextAnswer { set; get; }

		public int IdSurvey { set; get; }
		
		public List<Vote> Votes { set; get; }
	}
}
