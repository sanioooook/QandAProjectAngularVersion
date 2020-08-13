using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApiQandA.DTO
{
    public class SurveyDTO
    {
        [Required]
        public int? Id { get; set; }// ид опроса

        [Required]
        public string Question { set; get; }//текст вопроса для опроса

        [Required]
        public AnswerDTO[] Answers { get; set; }//пулл ответов

        public UserForPublic User { get; set; } //юзер-создатель опроса

        [Required]
        public bool SeveralAnswer { get; set; }//можно ли отвечать за несколько ответов

        [Required]
        public bool AddResponse { get; set; }//можно ли добавлять ответы посторонним личностям

        public DateTime TimeCreate { get; set; }//время создания, сервер сам заполняет

        public override bool Equals(object obj)
        {
            var temp = obj as SurveyDTO;
            if (temp != null && temp.Answers.Where((answerDto, i) => !answerDto.Equals(Answers[i])).Any())
            {
                return false;
            }

            return temp != null && (temp.TimeCreate == TimeCreate
                                    && temp.User.Equals(User)
                                    && temp.AddResponse == AddResponse
                                    && temp.SeveralAnswer == SeveralAnswer
                                    && temp.Question == Question);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Question, Answers, User, SeveralAnswer, AddResponse, TimeCreate);
        }
    }
}
