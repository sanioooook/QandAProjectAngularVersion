using System.Collections.Generic;
using Entities.Enums;
using Entities.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Interfaces
{
    public interface ISurveyService
    {
        void Create(SurveyDto surveyDto);

        SurveyDto GetSurveyBySurveyId(int surveyId);


        Pagination<SurveyDto> GetAllSurveys(Sort<SurveySortBy> sort, User user, Pagination<SurveyDto> pagination,
            Filter filter);

        void EditSurvey(SurveyDto surveyDto);

        void DeleteSurveyBySurveyId(int userId, int surveyId);

        int GetCountSurveys(string surveyQuestionFilter = null);

        SurveyDto GetSurveyBySurveyIdAndUserId(int surveyDtoId, int userId);

        bool IsUserVoteInSurvey(SurveyDto surveyDto, User user);
    }
}