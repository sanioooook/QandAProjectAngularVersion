using System;
using System.Collections.Generic;
using AutoMapper;
using Entities.Interfaces;
using Entities.Models;
using WebApiQandA.DTO;
using WebApiQandA.Interfaces;

namespace WebApiQandA.Services
{
    public class AnswerService: IAnswerService
    {
        private readonly IAnswerRepository _answerRepository;
        private readonly IMapper _mapper;
        private readonly IVoteService _voteService;

        public AnswerService(IAnswerRepository answerRepository, IMapper mapper, IVoteService voteService)
        {
            _answerRepository = answerRepository;
            _mapper = mapper;
            _voteService = voteService;
        }

        public AnswerDto Create(AnswerDto answerDto)
        {
            return _mapper.Map<AnswerDto>(_answerRepository.Create(_mapper.Map<Answer>(answerDto)));
        }
        
        public AnswerDto GetAnswerByAnswerId(int answerId)
        {
            return _mapper.Map<AnswerDto>(_answerRepository.GetAnswerByAnswerId(answerId));
        }

        public List<AnswerDto> GetAnswersBySurveyId(int surveyId)
        {
            var answersDto = new List<AnswerDto>();
            _answerRepository.GetAnswersBySurveyId(surveyId).ForEach(answer => answersDto.Add(_mapper.Map<AnswerDto>(answer)));
            return answersDto;
        }

        public List<AnswerDto> GetAllAnswers()
        {
            var answersDto = new List<AnswerDto>();
            _answerRepository.GetAllAnswers().ForEach(answer => answersDto.Add(_mapper.Map<AnswerDto>(answer)));
            return answersDto;
        }

        public void EditAnswer(AnswerDto answerDto)
        {
            if(answerDto.Id != null && GetAnswerByAnswerId((int)answerDto.Id) != null)
            {
                _answerRepository.EditAnswer(_mapper.Map<Answer>(answerDto));
            }
            else
            {
                throw new ArgumentException("Id answer wrong or answer not exist.");
            }
        }

        public void DeleteAnswerByAnswerId(int answerId)
        {
            if(GetAnswerByAnswerId(answerId)==null)
            {
                throw new ArgumentException($"Wrong answerId {answerId}");
            }
            _voteService.DeleteVotesByAnswerId(answerId);
            _answerRepository.DeleteAnswerByAnswerId(answerId);
        }

        public void DeleteAnswersBySurveyId(int surveyId)
        {
            _answerRepository.GetAnswersBySurveyId(surveyId)
                .ForEach(answer => DeleteAnswerByAnswerId(answer.Id));
        }
    }
}