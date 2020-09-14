import { Answer } from './answer';
import { UserForPublic } from './user-for-public';

export class Survey {

  public addResponse: boolean;

  public answers: Answer[];

  public id: number;

  public question: string;

  public severalAnswer: boolean;

  public user: UserForPublic;

  public timeCreate: Date;

  public abilityVoteFrom: Date;

  public abilityVoteTo: Date = null;
}
