﻿using System;
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
          surveyDto.AbilityVoteFrom = surveyDto.AbilityVoteFrom == DateTime.MinValue
                ? DateTime.Now.ToUniversalTime()
                : surveyDto.AbilityVoteFrom.ToUniversalTime();
            surveyDto.AbilityVoteFrom = surveyDto.AbilityVoteFrom.AddSeconds(surveyDto.AbilityVoteFrom.Second - 60);
            if(surveyDto.MaxCountVotes != null && surveyDto.MaxCountVotes > surveyDto.Answers.Count)
            {
                throw new ArgumentException("MaxCountVotes can't be more Answers");
            }
            if(surveyDto.AbilityVoteTo == DateTime.MinValue || surveyDto.AbilityVoteTo == null)
            {
                surveyDto.AbilityVoteTo = null;
            }
            else
            {
                var time = (DateTime)surveyDto.AbilityVoteTo;
                time = time.AddSeconds(time.Second - 60);
                surveyDto.AbilityVoteTo = time;
            }
            surveyDto.TimeCreate = DateTime.Now.ToUniversalTime();
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
        public Pagination<SurveyDto> GetAllSurveys(Sort<SurveySortBy> sort, User user, Pagination<SurveyDto> pagination,
            Filter filter)
        {
            var surveysDto = new List<SurveyDto>();
            if(pagination.PageNumber == null || pagination.PageSize == null)
            {
                if(pagination.PageNumber == null)
                {
                    throw new ArgumentNullException(nameof(pagination.PageNumber));
                }
                if(pagination.PageSize == null)
                {
                    throw new ArgumentNullException(nameof(pagination.PageSize));
                }
            }

            _surveyRepository
                .GetAllSurveys()
                .ForEach(survey =>
                {
                    var surveyDto = _mapper.Map<SurveyDto>(survey);
                    // ReSharper disable once AccessToModifiedClosure
                    surveysDto.Add(surveyDto);
                });
            switch(sort.SortBy)
            {
                case SurveySortBy.NumberAnswers:
                {
                    surveysDto.Sort((a, b) => a.Answers.Count.CompareTo(b.Answers.Count));
                    surveysDto = sort.SortDirection == SortDirection.Ascending
                        ? surveysDto.OrderBy(survey => survey.Answers.Count).ToList()
                        : surveysDto.OrderByDescending(survey => survey.Answers.Count).ToList();
                    break;
                }
                case SurveySortBy.NumberVotes:
                {
                    surveysDto.Sort((a, b) => GetCountVotesInSurvey(a).CompareTo(GetCountVotesInSurvey(b)));
                    surveysDto = sort.SortDirection == SortDirection.Ascending
                        ? surveysDto.OrderBy(GetCountVotesInSurvey).ToList()
                        : surveysDto.OrderByDescending(GetCountVotesInSurvey).ToList();
                    break;
                }
                case SurveySortBy.PermissionEdit:
                {
                    surveysDto.Sort((a, b) => IsUserSurvey(a, user).CompareTo(IsUserSurvey(b, user)));
                    surveysDto = sort.SortDirection == SortDirection.Ascending
                        ? surveysDto.OrderBy(survey => IsUserSurvey(survey, user)).ToList()
                        : surveysDto.OrderByDescending(survey => IsUserSurvey(survey, user)).ToList();
                    break;
                }
                case SurveySortBy.Question:
                {
                    surveysDto.Sort((a, b) => string.Compare(a.Question, b.Question, StringComparison.Ordinal));
                    surveysDto = sort.SortDirection == SortDirection.Ascending
                        ? surveysDto.OrderBy(survey => survey.Question).ToList()
                        : surveysDto.OrderByDescending(survey => survey.Question).ToList();
                    break;
                }
                case SurveySortBy.TimeCreate:
                {
                    surveysDto.Sort((a, b) => a.TimeCreate.CompareTo(b.TimeCreate));
                    surveysDto = sort.SortDirection == SortDirection.Ascending
                        ? surveysDto.OrderBy(survey => survey.TimeCreate).ToList()
                        : surveysDto.OrderByDescending(survey => survey.TimeCreate).ToList();
                    break;
                }
                case SurveySortBy.Id:
                {
                    surveysDto.Sort((a, b) => a.Id.Value.CompareTo(b.Id.Value));
                    surveysDto = sort.SortDirection == SortDirection.Ascending
                        ? surveysDto.OrderBy(survey => survey.Id).ToList()
                        : surveysDto.OrderByDescending(survey => survey.Id).ToList();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if(!string.IsNullOrWhiteSpace(filter.SearchQuery))
            {
                surveysDto = surveysDto.Where(survey => survey.Question.ToLower().Trim().Contains(filter.SearchQuery.ToLower().Trim())).ToList();
            }

            var returnsSurveys = new Pagination<SurveyDto>
            {
                TotalCount = surveysDto.Count, PageCount = surveysDto.Count / pagination.PageSize, PageSize = pagination.PageSize, PageNumber = pagination.PageNumber
            };
            surveysDto = surveysDto
                .Skip((int)(pagination.PageNumber * pagination.PageSize))
                .Take((int)pagination.PageSize).ToList();
            returnsSurveys.Data = surveysDto;

            return returnsSurveys;
        }

        public void EditSurvey(SurveyDto surveyDto)
        {
            if(surveyDto.MaxCountVotes != null &&
               // ReSharper disable once PossibleInvalidOperationException
               surveyDto.MaxCountVotes > _answerService.GetAnswersBySurveyId((int)surveyDto.Id).Count)
            {
                throw new ArgumentException("MaxCountVotes can't be more Answers");
            }
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

        public bool IsUserVoteInSurvey(SurveyDto surveyDto, User user)
        {
            return surveyDto.Answers.SelectMany(answer => answer.Votes).Any(vote => vote.Voter == user.Login);
        }

        private static bool IsUserSurvey(SurveyDto surveyDto, User user)
        {
            return surveyDto.User.Login == user.Login;
        }

        public SurveyDto GetSurveyBySurveyIdAndUserId(int surveyId, int userId)
        {
            return _mapper.Map<SurveyDto>(_surveyRepository.GetSurveyBySurveyIdAndUserId(surveyId, userId));
        }
    }
}
