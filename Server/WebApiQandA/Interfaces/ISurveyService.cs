using System.Collections.Generic;
using Entities.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Interfaces
{
    public interface ISurveyService
    {
        void Create(SurveyDto surveyDto);

        SurveyDto GetSurveyBySurveyId(int surveyId);


        List<SurveyDto> GetAllSurveys(Filtration filtration, User user,
            Pagination<SurveyDto> pagination);

        void EditSurvey(SurveyDto surveyDto);

        void DeleteSurveyBySurveyId(int userId, int surveyId);

        int GetCountSurveys(string surveyQuestionFilter = null);
    }
}