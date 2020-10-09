import { Vote } from './vote';

export class Answer {

  public id: number;

  public idSurvey: number;

  public textAnswer: string;

  public votes: Vote[];

  public isUserVote: boolean;

  constructor(idSurvey: number, textAnswer: string) {
    this.id = 0;
    this.idSurvey = idSurvey;
    this.isUserVote = false;
    this.textAnswer = textAnswer;
    this.votes = new Array<Vote>();
  }

}
