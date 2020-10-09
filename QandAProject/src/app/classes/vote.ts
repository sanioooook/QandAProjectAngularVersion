
export class Vote {

  public dateVote: Date;

  public id: number;

  public idAnswer: number;

  public voter: string;

  public isUserVote: boolean;

  public idSurvey: number;

  constructor(idAnswer: number, idSurvey: number) {
    this.dateVote = new Date();
    this.id = 0;
    this.idAnswer = idAnswer;
    this.idSurvey = idSurvey;
  }
}
