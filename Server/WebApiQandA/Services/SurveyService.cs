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
    public class SurveyService : ISurveyService
    {
        public SurveyService(ISurveyRepository surveyRepository,
                              IAnswerService answerService,
                              IMapper mapper)
        {
            _surveyRepository = surveyRepository;
            _answerService = answerService;
            _mapper = mapper;
        }

        private readonly ISurveyRepository _surveyRepository;
        private readonly IAnswerService _answerService;
        private readonly IMapper _mapper;

        public void Create(SurveyDto surveyDto)
        {
            surveyDto.TimeCreate = DateTime.Now; ;
            var survey = _surveyRepository.CreateSurvey(_mapper.Map<Survey>(surveyDto));
            surveyDto.Answers.ForEach(answer =>
            {
                answer.IdSurvey = survey.Id;
                _answerService.Create(answer);
            });

        }

        public SurveyDto GetSurveyBySurveyId(int surveyId)
        {
            return _mapper.Map<SurveyDto>(_surveyRepository.GetSurveyBySurveyId(surveyId));
        }
        
        public List<SurveyDto> GetAllSurveys(Filtration filtration, User user, Pagination<SurveyDto> pagination)
        {
            if(filtration == null)
            {
                throw new ArgumentNullException(nameof(filtration));
            }

            var surveysDto = new List<SurveyDto>();
            if(pagination.PageNumber != null&&pagination.PageSize != null)
            {
                _surveyRepository
                    .GetAllSurveys((int)pagination.PageNumber, (int)pagination.PageSize)
                    .ForEach(survey =>
                    {
                        var surveyDto = _mapper.Map<SurveyDto>(survey);
                        //    if(surveyFilterDto.Creator != null && surveyFilterDto.Creator == surveyDto.User.Login ||
                        //       surveyFilterDto.UserVote != null && IsUserVote(surveyDto, user) ||
                        //       surveyFilterDto.UserVote == null && surveyFilterDto.Creator == null)
                        //    {
                        surveysDto.Add(surveyDto);
                        //}
                    });
            }

            return surveysDto;
        }

        public void EditSurvey(SurveyDto surveyDto)
        {
            _surveyRepository.EditSurvey(_mapper.Map<Survey>(surveyDto));
        }

        public void DeleteSurveyBySurveyId(int userId, int surveyId)
        {
            _answerService.DeleteAnswersBySurveyId(surveyId);
            _surveyRepository.DeleteSurveyBySurveyId(userId, surveyId);
        }

        private bool IsUserVote(SurveyDto surveyDto, User user)
        {
            return surveyDto.Answers.SelectMany(answer => answer.Votes).Any(vote => vote.Voter == user.Login);
        }

        public int GetCountSurveys(string surveyQuestionFilter = null)
        {
            return _surveyRepository.GetCountSurveys(surveyQuestionFilter);
        }
    }
}
