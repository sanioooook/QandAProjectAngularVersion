using System;
using System.ComponentModel.DataAnnotations;

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
    }
}
