﻿using System.Collections.Generic;
using Entities.Enums;
using Entities.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Interfaces
{
    public interface ISurveyService
    {
        void Create(SurveyDto surveyDto);

        SurveyDto GetSurveyBySurveyId(int surveyId);


        IEnumerable<SurveyDto> GetAllSurveys(Sort<SurveySortBy> sort, User user, Pagination<SurveyDto> pagination, Filter filter);

        void EditSurvey(SurveyDto surveyDto);

        void DeleteSurveyBySurveyId(int userId, int surveyId);

        int GetCountSurveys(string surveyQuestionFilter = null);
    }
}