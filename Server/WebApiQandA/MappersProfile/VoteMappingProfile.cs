using System;
using AutoMapper;
using Entities.Models;
using WebApiQandA.DTO;
using WebApiQandA.Interfaces;

namespace WebApiQandA.MappersProfile
{
    public class VoteMappingProfile: Profile
    {
        public VoteMappingProfile()
        {
            CreateMap<Vote, VoteDto>()
                .AfterMap<SetVoteAction>();

            CreateMap<VoteDto, Vote>()
                .AfterMap<SetVoteDtoAction>();
        }
    }

    public class SetVoteAction : IMappingAction<Vote, VoteDto>
    {
        private readonly IUserService _userService;

        public SetVoteAction(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public void Process(Vote source, VoteDto destination, ResolutionContext context)
        {
            destination.Voter = _userService.GetUserById(source.IdCustomer).Login;
        }
    }

    public class SetVoteDtoAction : IMappingAction<VoteDto, Vote>
    {
        private readonly IUserService _userService;

        public SetVoteDtoAction(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public void Process(VoteDto source, Vote destination, ResolutionContext context)
        {
            destination.IdCustomer = _userService.GetUserByLogin(source.Voter).Id;
        }
    }
}