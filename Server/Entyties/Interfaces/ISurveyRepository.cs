using System.Collections.Generic;
using Entities.Models;

namespace Entities.Interfaces
{
    public interface ISurveyRepository
    {
        Survey CreateSurvey(Survey survey);

        Survey GetSurveyBySurveyId(int id);

        int GetCountSurveys(string surveyQuestionFilter = null);

        List<Survey> GetAllSurveys(int page, int pageSize);
        
        void EditSurvey(Survey surveyDto);

        void DeleteSurveyBySurveyId(int userId, int surveyId);
    }
}
