using System.Collections.Generic;
using WebApiQandA.DTO;

namespace WebApiQandA.Interfaces
{
    public interface ISurveyService
    {
        void Create(SurveyDto surveyDto);

        SurveyDto GetSurveyBySurveyId(int surveyId);

        List<SurveyDto> GetSurveysByUser(int userId);

        List<SurveyDto> GetAllSurveys();

        void EditSurvey(SurveyDto surveyDto);

        void DeleteSurveyBySurveyId(int userId, int surveyId);
    }
}