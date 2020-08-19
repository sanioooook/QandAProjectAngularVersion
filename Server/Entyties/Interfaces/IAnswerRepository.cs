using System.Collections.Generic;
using Entities.Models;

namespace Entities.Interfaces
{
	public interface IAnswerRepository
	{
        Answer Create(Answer answer);

        Answer GetAnswerByAnswerId(int answerId);

        List<Answer> GetAnswersBySurveyId(int surveyId);

        List<Answer> GetAllAnswers();

        void EditAnswer(Answer answer);

        void DeleteAnswerByAnswerId(int answerId);

        void DeleteAnswersBySurveyId(int surveyId);
    }
}
