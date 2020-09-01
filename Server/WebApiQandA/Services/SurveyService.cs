using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoMapper;
using Entities.Enums;
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
            surveyDto.TimeCreate = DateTime.Now;
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
        
        [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
        public IEnumerable<SurveyDto> GetAllSurveys(Sort<SurveySortBy> sort, User user, Pagination<SurveyDto> pagination)
        {
            var surveysDto = new List<SurveyDto>();
            if(pagination.PageNumber != null && pagination.PageSize != null)
            {

                _surveyRepository
                    .GetAllSurveys()
                    .ForEach(survey => surveysDto.Add(_mapper.Map<SurveyDto>(survey)));
                switch(sort.SortBy)
                {
                    case SurveySortBy.NumberAnswers:
                    {
                        surveysDto.Sort((a, b) => a.Answers.Count.CompareTo(b.Answers.Count));
                        if(sort.SortDirection == SortDirection.Ascending)
                        {
                            surveysDto.OrderBy(survey => survey.Answers.Count);
                        }
                        else
                        {
                            surveysDto = (from surveyDto in surveysDto orderby surveyDto.Answers.Count descending select surveyDto).ToList();
                        }
                        break;
                    }
                    case SurveySortBy.NumberVotes:
                    {
                        surveysDto.Sort((a, b) => GetCountVotesInSurvey(a).CompareTo(GetCountVotesInSurvey(b)));
                        if(sort.SortDirection == SortDirection.Ascending)
                        {
                            surveysDto.OrderBy(GetCountVotesInSurvey);
                        }
                        else
                        {
                           /* surveysDto.OrderByDescending(GetCountVotesInSurvey);*/
                            surveysDto = (from surveyDto in surveysDto orderby GetCountVotesInSurvey(surveyDto) descending select surveyDto).ToList();
                        }
                        break;
                    }
                    case SurveySortBy.PermissionEdit:
                    {
                        surveysDto.Sort((a, b) => IsUserSurvey(a, user).CompareTo(IsUserSurvey(b, user)));
                        if(sort.SortDirection == SortDirection.Ascending)
                        {
                            surveysDto.OrderBy(survey => IsUserSurvey(survey, user));
                        }
                        else
                        {
                            surveysDto = (from surveyDto in surveysDto orderby IsUserSurvey(surveyDto, user) descending select surveyDto).ToList();
                        }
                        break;
                    }
                    case SurveySortBy.Question:
                    {
                        surveysDto.Sort((a, b) => string.Compare(a.Question, b.Question, StringComparison.Ordinal));
                        if(sort.SortDirection == SortDirection.Ascending)
                        {
                            surveysDto.OrderBy(survey => survey.Question);
                        }
                        else
                        {
                            surveysDto = (from surveyDto in surveysDto orderby surveyDto.Question descending select surveyDto).ToList();
                        }
                        break;
                    }
                    case SurveySortBy.TimeCreate:
                    {
                        surveysDto.Sort((a, b) => a.TimeCreate.CompareTo(b.TimeCreate));
                        if(sort.SortDirection == SortDirection.Ascending)
                        {
                            surveysDto.OrderBy(survey => survey.TimeCreate);
                        }
                        else
                        {
                            surveysDto = (from surveyDto in surveysDto orderby surveyDto.TimeCreate descending select surveyDto).ToList();
                        }
                        break;
                    }
                    default:
                    {
                        surveysDto.Sort((a, b) => a.Id.Value.CompareTo(b.Id.Value));
                        if(sort.SortDirection == SortDirection.Ascending)
                        {
                            surveysDto.OrderBy(survey => survey.Id);
                        }
                        else
                        {
                            surveysDto.OrderByDescending(survey => survey.Id);
                            surveysDto = (from surveyDto in surveysDto orderby surveyDto.Id descending select surveyDto).ToList();
                        }
                        break;
                    }
                }
                surveysDto = surveysDto
                    .Skip((int)(pagination.PageNumber * pagination.PageSize))
                    .Take((int)pagination.PageSize)
                    .ToList();
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

        public int GetCountSurveys(string surveyQuestionFilter = null)
        {
            return _surveyRepository.GetCountSurveys(surveyQuestionFilter);
        }

        private static int GetCountVotesInSurvey(SurveyDto surveyDto)
        {
            return surveyDto.Answers.Sum(answer => answer.Votes.Count);
        }

        private static bool IsUserSurvey(SurveyDto surveyDto, User user)
        {
            return surveyDto.User.Login == user.Login;
        }
    }
}
