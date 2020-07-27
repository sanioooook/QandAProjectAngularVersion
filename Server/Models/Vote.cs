using System;

namespace Server2.Models
{
	public class Vote//голос
	{
		public int IdAnswer { get; set; }//ид ответа за который отдан голос
		public int IdCustomer { get; set; }//ид юзер проголосовавшего за ответ
		public Answer Answer { get; set; }//за что голосовал
		public User Voter { get; set; }//кто голосовал
		public DateTime DateVote { get; set; }//когда голосовал
	}
}
