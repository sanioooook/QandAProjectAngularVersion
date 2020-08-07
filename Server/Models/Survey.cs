using System.ComponentModel.DataAnnotations.Schema;

namespace Server2.Models
{
	public class Survey//опрос
	{
		public int Id { get; set; }// ид опроса
		
		public string Question { set; get; }//текст вопроса для опроса
		
		public int IdCreator { get; set; }//ид создавшего опрос

		[ForeignKey("IdCreator")]
		public User God { get; set; }//создавший этот опрос
		
		public Answer[] Answers { get; set; }//пулл ответов

		public bool SeveralAnswer { get; set; }//можно ли отвечать за несколько ответов

		public bool AddResponse { get; set; }//можно ли добавлять ответы посторонним личностям

	}
}
