using System;
using System.ComponentModel.DataAnnotations;

namespace WebApiQandA.DTO
{
    public class VoteDTO
    {
        [Required]
        public int? Id { get; set; }

        [Required]
        public int? IdAnswer { get; set; }//ид ответа за который отдан голос

        [Required]
        public string Voter { get; set; }//кто голосовал

        public DateTime DateVote { get; set; }//когда голосовал
    }
}
