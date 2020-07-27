using System.Collections.Generic;
using Server2.Models;

namespace WebApiQandA.Models.Interfaces
{
	public interface IAnswerRepository
	{
		string Create(string token, Answer answer);
		Answer Get(string token, int id);
		List<Answer> Get(string token, Survey survey);
		List<Answer> Get(string token, string answer);
		List<Answer> GetAnswers(string token);
	}
}
