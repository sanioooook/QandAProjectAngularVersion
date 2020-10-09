using System.Collections.Generic;
using Entities.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Interfaces
{
    public interface IVoteService
    {
        void Create(VoteDto[] votesDto);

        VoteDto GetVoteByVoteId(int voteId);

        List<VoteDto> GetAllVotes();

        List<VoteDto> GetVotesByUserId(int userId);

        void DeleteVotesByAnswerId(int answerId);

        List<VoteDto> GetVotesByAnswerId(int answerId);
    }
}