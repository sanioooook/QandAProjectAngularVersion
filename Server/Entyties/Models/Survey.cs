using System;

namespace Entities.Models
{
    public class Survey
    {
		public int Id { get; set; }
		
		public string Question { set; get; }
		
		public int IdCreator { get; set; }
		
		public bool SeveralAnswer { get; set; }

		public bool AddResponse { get; set; }

        public DateTime TimeCreate { get; set; }

		public DateTime AbilityVoteFrom { get; set; }

		public DateTime? AbilityVoteTo { get; set; }
	}
}
