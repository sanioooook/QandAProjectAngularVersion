using System.Collections.Generic;
using WebApiQandA.DTO;

namespace WebApiQandA.Interfaces
{
    public interface IAnswerService
    {
        AnswerDto Create(AnswerDto answerDto);

        AnswerDto GetAnswerByAnswerId(int answerId);

        List<AnswerDto> GetAnswersBySurveyId(int surveyId);

        List<AnswerDto> GetAllAnswers();

        void EditAnswer(AnswerDto answerDto);

        void DeleteAnswerByAnswerId(int answerId);

        void DeleteAnswersBySurveyId(int surveyId);
    }
}