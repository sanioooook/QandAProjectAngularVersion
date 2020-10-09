import { Answer } from './answer';
import { UserForPublic } from './user-for-public';

export class Survey {

  public addResponse: boolean;

  public answers: Answer[];

  public id: number;

  public question: string;

  public user: UserForPublic;

  public timeCreate: Date;

  public abilityVoteFrom: Date;

  public abilityVoteTo: Date = null;

  public minCountVotes: number;

  public maxCountVotes: number = null;

  constructor(
    question: string,
    answers: Answer[],
    addResponse: boolean,
    abilityVoteFrom: Date,
    minCountVotes: number,
    maxCountVotes: number = null,
    abilityVoteTo: Date = null
    ) {
      this.abilityVoteFrom = abilityVoteFrom;
      this.minCountVotes = minCountVotes;
      this.maxCountVotes = maxCountVotes;
      this.abilityVoteTo = abilityVoteTo;
      this.addResponse = addResponse;
      this.question = question;
      this.answers = answers;
      this.id = 0;
  }
}
