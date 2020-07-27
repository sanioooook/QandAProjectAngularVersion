using System.Collections.Generic;
using Server2.Models;

namespace WebApiQandA.Models.Interfaces
{
	public interface IVoteRepository
	{
		string Create(string token, Vote vote);
		Vote Get(string token, int id);
		List<Vote> GetVotes(string token);
		List<Vote> Get(string token, User user);
		List<Answer> Get(string token, Answer[] answer);
	}
}
