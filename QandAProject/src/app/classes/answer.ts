import { Vote } from './vote';

export class Answer {

  public id: number;

  public idSurvey: number;

  public textAnswer: string;

  public votes: Vote[];

  public isUserVote: boolean;
}
