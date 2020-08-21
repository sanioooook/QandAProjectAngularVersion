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

        public VoteDto Create(VoteDto vote)
        {
            var answers = new List<AnswerDto>();
            var answer = _answerRepository.GetAnswerByAnswerId((int)vote.IdAnswer);
            if(answer != null)
            {
                _answerRepository.GetAnswersBySurveyId(answer.IdSurvey)
                    .ForEach(answer => answers.Add(_mapper.Map<AnswerDto>(answer)));
                var survey = _surveyRepository.GetSurveyBySurveyId((int)answers[0].IdSurvey);
                if(answers.SelectMany(answerDto => answerDto.Votes).Any(voteDto =>
                    voteDto.Voter == vote.Voter && voteDto.IdAnswer == vote.IdAnswer))
                {
                    throw new Exception("You can't vote for the same option");
                }

                if(!survey.SeveralAnswer &&
                   answers.Any(answer => answer.Votes.Find(voteDto => voteDto.Voter == vote.Voter) != null))
                {
                    throw new Exception("You don't have permissions for multi vote");
                }
            }
            else
            {
                throw new Exception("Don't have this answer in database");
            }
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