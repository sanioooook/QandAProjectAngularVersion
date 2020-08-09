using System;
using System.ComponentModel.DataAnnotations.Schema;
using Server2.Models;

namespace WebApiQandA.Models
{
	public class Survey//опрос
	{
		public int Id { get; set; }// ид опроса
		
		public string Question { set; get; }//текст вопроса для опроса
		
		public int IdCreator { get; set; }//ид создавшего опрос
		
		public Answer[] Answers { get; set; }//пулл ответов

		public bool SeveralAnswer { get; set; }//можно ли отвечать за несколько ответов

		public bool AddResponse { get; set; }//можно ли добавлять ответы посторонним личностям

        public DateTime TimeCreate { get; set; }//время создания, сервер сам заполняет
	}
}
