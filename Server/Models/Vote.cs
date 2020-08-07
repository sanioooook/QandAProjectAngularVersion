using System;

namespace Server2.Models
{
	public class Vote//голос
	{
		public int Id { get; set; }

		public int IdAnswer { get; set; }//ид ответа за который отдан голос

		public int IdCustomer { get; set; }//ид юзер проголосовавшего за ответ

		public DateTime DateVote { get; set; }//когда голосовал

	}
}
