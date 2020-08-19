using System.Collections.Generic;
using Entities.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Interfaces
{
    public interface ISurveyService
    {
        void Create(SurveyDto surveyDto);

        SurveyDto GetSurveyBySurveyId(int surveyId);

        List<SurveyDto> GetSurveysByUser(int userId);

        List<SurveyDto> GetAllSurveys();

        bool IsUserVote(User user, int surveyId);

        void EditSurvey(SurveyDto surveyDto);

        void DeleteSurveyBySurveyId(int userId, int surveyId);
    }
}