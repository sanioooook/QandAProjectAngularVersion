using System;
using AutoMapper;
using Entities.Models;
using WebApiQandA.DTO;
using WebApiQandA.Interfaces;

namespace WebApiQandA.MappersProfile
{
    public class SurveyMappingProfile : Profile
    {
        public SurveyMappingProfile()
        {
            CreateMap<Survey, SurveyDto>()
                .AfterMap<SetSurveyAction>();

            CreateMap<SurveyDto, Survey>()
                .AfterMap<SetSurveyDtoAction>();
        }
    }

    public class SetSurveyAction : IMappingAction<Survey, SurveyDto>
    {
        private readonly IAnswerService _answerService;
        private readonly IUserService _userService;

        public SetSurveyAction(IAnswerService answerService,
                    IUserService userService)
        {
            _answerService = answerService ?? throw new ArgumentNullException(nameof(answerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public void Process(Survey source, SurveyDto destination, ResolutionContext context)
        {
            destination.Answers = _answerService.GetAnswersBySurveyId(source.Id);
            destination.User = _userService.GetUserById(source.IdCreator);
        }
    }

    public class SetSurveyDtoAction : IMappingAction<SurveyDto, Survey>
    {
        private readonly IUserService _userService;

        public SetSurveyDtoAction(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public void Process(SurveyDto source, Survey destination, ResolutionContext context)
        {
            destination.IdCreator = _userService.GetUserByLogin(source.User.Login).Id;
        }
    }

}