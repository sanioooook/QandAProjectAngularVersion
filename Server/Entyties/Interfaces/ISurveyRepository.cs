using System.Collections.Generic;
using Entities.Models;

namespace Entities.Interfaces
{
    public interface ISurveyRepository
    {
        Survey CreateSurvey(Survey survey);

        Survey GetSurveyBySurveyId(int id);

        List<Survey> GetSurveysByUserId(int userId);

        List<Survey> GetAllSurveys();
        
        void EditSurvey(Survey surveyDto);

        void DeleteSurveyBySurveyId(int userId, int surveyId);
    }
}
