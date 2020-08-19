using System;
using AutoMapper;
using Entities.Interfaces;
using Entities.Models;
using WebApiQandA.DTO;
using WebApiQandA.Interfaces;

namespace WebApiQandA.MappersProfile
{
    public class AnswerMappingProfile: Profile
    {
        public AnswerMappingProfile()
        {
            CreateMap<Answer, AnswerDto>()
                .AfterMap<SetAnswerAction>();

            CreateMap<AnswerDto, Answer>()
                .AfterMap<SetAnswerDtoAction>();
        }
    }

    public class SetAnswerAction : IMappingAction<Answer, AnswerDto>
    {
        private readonly IVoteService _voteService;

        public SetAnswerAction(IVoteService voteService)
        {
            _voteService = voteService ?? throw new ArgumentNullException(nameof(voteService));
        }

        public void Process(Answer source, AnswerDto destination, ResolutionContext context)
        {
            destination.Votes = _voteService.GetVotesByAnswerId(source.Id);
        }
    }

    public class SetAnswerDtoAction : IMappingAction<AnswerDto, Answer>
    {
        private readonly IVoteRepository _voteRepository;

        public SetAnswerDtoAction(IVoteRepository voteRepository)
        {
            _voteRepository = voteRepository ?? throw new ArgumentNullException(nameof(voteRepository));
        }

        public void Process(AnswerDto source, Answer destination, ResolutionContext context)
        {
            destination.Votes = _voteRepository.GetVotesByAnswerId((int)source.Id);
        }
    }
}