using System.Collections.Generic;
using Server2.Models;
using WebApiQandA.DTO;

namespace WebApiQandA.Models.Interfaces
{
	public interface IAnswerRepository
	{
		void Create(AnswerDTO answer);

		AnswerDTO GetAnswerById(int id);

		List<AnswerDTO> GetAnswersBySurvey(Survey survey);

		List<AnswerDTO> GetAnswersBySurveyId(int surveyId);

		List<AnswerDTO> GetAllAnswersByAnswer(string answer);

		List<AnswerDTO> GetAllAnswers();

	}
}
