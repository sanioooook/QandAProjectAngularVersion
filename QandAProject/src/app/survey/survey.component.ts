import { Component, OnInit } from '@angular/core';
import { Survey } from '../classes/survey';
import { ActivatedRoute } from '@angular/router';
import * as moment from 'moment';
import { switchMap } from 'rxjs/operators';
import { SurveysService } from '../services/surveys.service';
import { HttpErrorResponse } from '@angular/common/http';
import { Answer } from '../classes/answer';
import { Vote } from '../classes/vote';
import { UserForPublic } from '../classes/user-for-public';
import { UserService } from '../services/user-service.service';
import { MatCheckboxChange } from '@angular/material/checkbox';

@Component({
  selector: 'app-survey',
  templateUrl: './survey.component.html',
  styleUrls: ['./survey.component.css']
})
export class SurveyComponent implements OnInit {
  constructor(private route: ActivatedRoute,
              private surveyService: SurveysService,
              private userService: UserService) { }

  user = new UserForPublic();
  newAnswer: string;
  survey: Survey;
  isUserVote = false;
  votes = new Array<Vote>();

  ngOnInit(): void {
    this.userService.getUserLogin().then(login => this.user.login = login);
    this.route.paramMap.pipe(
      switchMap(params => params.getAll('id')))
      .subscribe(id => this.setSurvey(+id));
  }

  private setIsUserVote(): void {
    for (const answer of this.survey.answers) {
      if (answer.isUserVote) {
        this.isUserVote = true;
      }
    }
  }

  public addAnswer(): void {
    if (this.newAnswer && !this.isUserVote) {
      this.surveyService.AddNewAnswer(new Answer(this.survey.id, this.newAnswer))
        .then((newAnswer: Answer) => {
          this.survey.answers.push(newAnswer);
          this.newAnswer = '';
        })
        .catch((Error: HttpErrorResponse) => console.log(Error.error));
    }
  }

  public vote(e: MatCheckboxChange, answerId: number): void {
    const indexAnswer = this.survey.answers.findIndex(answer => answer.id === answerId);
    if (this.getAbilityVote(this.survey.answers[indexAnswer])) {
      const answer = this.survey.answers.find(answerLocal => answerLocal.id === answerId);
      if (e.checked && !answer.isUserVote) {
        answer.isUserVote = true;
        this.votes.push(new Vote(answerId, this.survey.id));
      }
      else if (!e.checked && answer.isUserVote) {
        answer.isUserVote = false;
        this.votes.splice(this.votes.findIndex(vote => vote.idAnswer === answerId), 1);
      }
    }
  }

  private setSurvey(id: number): void {
    this.surveyService.GetSurveyById(id)
      .then(survey => {
        this.survey = survey;
        this.setIsUserVote();
      })
      .catch((Error: HttpErrorResponse) => console.log(Error.error));
  }

  public getAbilityVote(answer: Answer): boolean {
    return !this.isUserVote
      && (this.abilityCountVote()
        && this.getAbilityDateTimeVote()
        || answer.isUserVote);
  }

  public getAbilityDateTimeVote(): boolean {
    if (new Date() >= moment(this.survey.abilityVoteFrom).local().toDate()
      && this.survey.abilityVoteTo
      && new Date() <= moment(this.survey.abilityVoteTo).local().toDate()) {
      return true;
    }
    else if (new Date() >= moment(this.survey.abilityVoteFrom).local().toDate()
      && !this.survey.abilityVoteTo) {
      return true;
    }
    return false;
  }

  abilityCountVote(): boolean {
    const voteCount = this.voteCount();
    const maxCountVotes = this.survey.maxCountVotes;
    if (maxCountVotes && voteCount < maxCountVotes || !maxCountVotes) {
      return true;
    }
    return false;
  }

  getDeadtimeTo(date: Date): string {
    return moment(date).toNow();
  }

  getDeadtimeFrom(date: Date): string {
    return moment(date).fromNow();
  }

  sendVotes(): void {
    const voteCount = this.voteCount();
    const maxCountVotes = this.survey.maxCountVotes;
    const minCountVotes = this.survey.minCountVotes;
    if (minCountVotes <= voteCount
      && maxCountVotes
      && voteCount <= maxCountVotes
      || !maxCountVotes
      && minCountVotes <= voteCount) {
      this.surveyService.Vote(this.votes)
        .then()
        .catch((Error: HttpErrorResponse) => console.log(Error.error))
        .finally(() => this.ngOnInit());
    }
  }

  private voteCount(): number {
    return this.survey.answers.filter(answer => answer.isUserVote).length;
  }

}
