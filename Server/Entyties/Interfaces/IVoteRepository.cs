using System.Collections.Generic;
using Entities.Models;

namespace Entities.Interfaces
{
	public interface IVoteRepository
	{
        Vote Create(Vote vote);

        Vote GetVoteByVoteId(int voteId);

        List<Vote> GetAllVotes();

        List<Vote> GetVotesByUserId(int userId);

        void DeleteVotesByAnswerId(int answerId);

        List<Vote> GetVotesByAnswerId(int answerId);
    }
}
