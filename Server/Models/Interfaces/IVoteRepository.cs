using System.Collections.Generic;
using Server2.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Models.Interfaces
{
	public interface IVoteRepository
	{
		bool Create(VoteDTO vote);

		VoteDTO GetVoteById(int id);

		List<VoteDTO> GetAllVotes();

		List<VoteDTO> GetVotesByUser(User user);

		List<VoteDTO> GetVotesByUserId(int userId);

		List<Answer> FillVotesInAnswers(Answer[] answers);
	}
}
