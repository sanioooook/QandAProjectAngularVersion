using System.Collections.Generic;
using Server2.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Models.Interfaces
{
    public interface ISurveyRepository
    {
        bool Create(User user, SurveyDTO survey);

        SurveyDTO GetSurveyBySurveyId(int id);

        List<SurveyDTO> GetSurveysByUser(User God);

        List<SurveyDTO> GetSurveysByUserAndQuestion(User user, string Question);

        List<SurveyDTO> GetAllSurveys();

        bool VoteUser(User user, SurveyDTO survey);

        Survey ConvertSurveyDTOToSurvey(SurveyDTO surveyDTO);

        void EditSurvey(SurveyDTO surveyDto);

        void DeleteSurveyBySurveyId(User deleter, int surveyId);
    }
}
