using System.Collections.Generic;
using Server2.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Models.Interfaces
{
	public interface IAnswerRepository
	{
		int Create(AnswerDTO answer);

		AnswerDTO GetAnswerById(int id);

		List<AnswerDTO> GetAnswersBySurvey(Survey survey);

		List<AnswerDTO> GetAnswersBySurveyId(int surveyId);

		List<AnswerDTO> GetAllAnswersByTextAnswer(string answer);

		List<AnswerDTO> GetAllAnswers();

        void DeleteAnswerByAnswerId(User user, int answerId);

        void EditAnswer(User user, AnswerDTO answerDto);

        void DeleteAnswersBySurveyId(int surveyId);

    }
}
