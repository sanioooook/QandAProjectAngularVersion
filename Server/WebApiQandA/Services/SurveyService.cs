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

        public List<SurveyDto> GetSurveysByUser(int userId)
        {
            var surveysDto = new List<SurveyDto>();
            _surveyRepository.GetSurveysByUserId(userId)
                .ForEach(survey => surveysDto.Add(_mapper.Map<SurveyDto>(survey)));
            return surveysDto;
        }

        public List<SurveyDto> GetAllSurveys()
        {
            var surveysDto = new List<SurveyDto>();
            _surveyRepository.GetAllSurveys()
                .ForEach(survey => surveysDto.Add(_mapper.Map<SurveyDto>(survey)));
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
    }
}
