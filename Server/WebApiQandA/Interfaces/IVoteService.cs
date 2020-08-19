using System.Collections.Generic;
using WebApiQandA.DTO;

namespace WebApiQandA.Interfaces
{
    public interface IVoteService
    {
        VoteDto Create(VoteDto vote);

        VoteDto GetVoteByVoteId(int voteId);

        List<VoteDto> GetAllVotes();

        List<VoteDto> GetVotesByUserId(int userId);

        void DeleteVotesByAnswerId(int answerId);

        List<VoteDto> GetVotesByAnswerId(int answerId);
    }
}