using System.Collections.Generic;
using AutoMapper;
using Entities.Interfaces;
using Entities.Models;
using WebApiQandA.DTO;
using WebApiQandA.Interfaces;

namespace WebApiQandA.Services
{
    public class VoteService : IVoteService
    {
        private readonly IVoteRepository _voteRepository;
        private readonly IMapper _mapper;

        public VoteService(IVoteRepository voteRepository,
            IMapper mapper)
        {
            _voteRepository = voteRepository;
            _mapper = mapper;
        }

        public VoteDto Create(VoteDto vote)
        {
            return _mapper.Map<VoteDto>(_voteRepository.Create(_mapper.Map<Vote>(vote)));
        }

        public VoteDto GetVoteByVoteId(int voteId)
        {
            return _mapper.Map<Vote, VoteDto>(_voteRepository.GetVoteByVoteId(voteId));
        }

        public List<VoteDto> GetAllVotes()
        {
            var votes = new List<VoteDto>();
            _voteRepository.GetAllVotes().ForEach(vote => votes.Add(_mapper.Map<Vote, VoteDto>(vote)));
            return votes;
        }

        public List<VoteDto> GetVotesByUserId(int userId)
        {
            var votes = new List<VoteDto>();
            _voteRepository.GetVotesByUserId(userId).ForEach(vote => votes.Add(_mapper.Map<Vote, VoteDto>(vote)));
            return votes;
        }

        public void DeleteVotesByAnswerId(int answerId)
        {
            _voteRepository.DeleteVotesByAnswerId(answerId);
        }

        public List<VoteDto> GetVotesByAnswerId(int answerId)
        {
            var votes = new List<VoteDto>();
            _voteRepository.GetVotesByAnswerId(answerId).ForEach(vote => votes.Add(_mapper.Map<Vote, VoteDto>(vote)));
            return votes;
        }
    }
}