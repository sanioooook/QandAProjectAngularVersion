using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ISurveyRepository _surveyRepository;
        private readonly IAnswerRepository _answerRepository;

        public VoteService(IVoteRepository voteRepository,
                            IMapper mapper,
                            ISurveyRepository surveyRepository,
                            IAnswerRepository answerRepository)
        {
            _voteRepository = voteRepository;
            _mapper = mapper;
            _answerRepository = answerRepository;
            _surveyRepository = surveyRepository;
        }

        public void Create(VoteDto[] votesDto)
        {
            var answers = new List<AnswerDto>();
            _answerRepository.GetAnswersBySurveyId(_answerRepository.GetAnswerByAnswerId((int)votesDto[0].IdAnswer).IdSurvey)
                .ForEach(source => answers.Add(_mapper.Map<AnswerDto>(source)));
            var surveyDto = _mapper.Map<SurveyDto>(_surveyRepository.GetSurveyBySurveyId(votesDto[0].IdSurvey));
            foreach(var vote in votesDto)
            {
                if(surveyDto.Answers.SelectMany(localAnswer => localAnswer.Votes).Any(dto => dto.Voter == vote.Voter))
                {
                    throw new Exception("You can't vote multiple times");
                }

                if(answers.SelectMany(answerDto => answerDto.Votes).Any(localVote =>
                    localVote.Voter == vote.Voter && localVote.IdAnswer == vote.IdAnswer))
                {
                    throw new Exception("You can't vote for the same option");
                }
            }
            if(surveyDto.MaxCountVotes != null && votesDto.Length > surveyDto.MaxCountVotes || votesDto.Length < surveyDto.MinCountVotes)
            {
                throw new Exception("More votes than the maximum or minimum allowed");
            }
            if(DateTime.Now.ToUniversalTime() < surveyDto.AbilityVoteFrom && surveyDto.AbilityVoteTo > DateTime.Now.ToUniversalTime())
            {
                throw new Exception("You can't vote, because time is out");
            }
            foreach(var vote in votesDto)
            {
                _mapper.Map<VoteDto>(_voteRepository.Create(_mapper.Map<Vote>(vote)));
            }
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