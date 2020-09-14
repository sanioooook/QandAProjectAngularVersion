using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebApiQandA.DTO
{
    public class SurveyDto
    {
        [Required]
        public int? Id { get; set; }

        [Required]
        public string Question { set; get; }

        [Required]
        public List<AnswerDto> Answers { get; set; }

        public UserForPublic User { get; set; }

        [Required]
        public bool SeveralAnswer { get; set; }

        [Required]
        public bool AddResponse { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime TimeCreate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AbilityVoteFrom { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? AbilityVoteTo { get; set; }

        public override bool Equals(object obj)
        {
            var temp = obj as SurveyDto;
            if (temp != null && temp.Answers.Where((answerDto, i) => !answerDto.Equals(Answers[i])).Any())
            {
                return false;
            }

            return temp != null && (temp.TimeCreate == TimeCreate
                                    && temp.User.Equals(User)
                                    && temp.AddResponse == AddResponse
                                    && temp.SeveralAnswer == SeveralAnswer
                                    && temp.Question == Question
                                    && temp.AbilityVoteFrom == AbilityVoteFrom
                                    && temp.AbilityVoteTo == AbilityVoteTo);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Question, Answers, User, SeveralAnswer, AddResponse, TimeCreate);
        }
    }
}
