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
  voteCountInSurvey: number;

  ngOnInit(): void {
    this.userService.getUserLogin().then(login => this.user.login = login);
    this.route.paramMap.pipe(
      switchMap(params => params.getAll('id')))
      .subscribe(id => this.setSurvey(+id));
  }

  isVote(): boolean {
    for (const answer of this.survey.answers) {
      if (answer.isUserVote) {
        return true;
      }
    }
    return false;
  }

  addAnswer(surveyId: number): void {
    const answer = new Answer();
    answer.textAnswer = this.newAnswer;
    answer.idSurvey = surveyId;
    answer.id = 0;
    this.surveyService.AddNewAnswer(answer)
      .then((newAnswer: Answer) => {
        this.survey.answers.push(newAnswer);
        this.newAnswer = '';
      })
      .catch((Error: HttpErrorResponse) => console.log(Error.error));
  }

  vote(answerId: number): void {
    const vote = new Vote();
    vote.id = 0;
    vote.idAnswer = answerId;
    this.surveyService.Vote(vote)
      .then()
      .catch((Error: HttpErrorResponse) => console.log(Error.error))
      .finally(() => this.ngOnInit());
  }

  processingSurvey(): void {
    this.voteCountInSurvey = 0;
    this.survey.answers.forEach(answer => {
      answer.isUserVote = false;
      this.voteCountInSurvey += answer.votes.length;
      if (answer.votes.length > 0) {
        answer.votes.forEach(voteInAnswer => {
          if (!answer.isUserVote && voteInAnswer.voter === this.user.login) {
            answer.isUserVote = true;
          }
        });
      }
    });
  }

  setSurvey(id: number): void {
    this.surveyService.GetSurveyById(id)
      .then(survey => {
        this.survey = survey;
        this.processingSurvey();
      })
      .catch((Error: HttpErrorResponse) => console.log(Error.error));
  }

  getNowDate(): Date {
    return new Date();
  }

  getAbilityVote(answer: Answer): boolean {
    return this.abilityMultivote() && this.abilityDateTimeVote() && !answer.isUserVote;
  }

  abilityDateTimeVote(): boolean {
    if (new Date() >= this.survey.abilityVoteFrom
      && this.survey.abilityVoteTo
      && new Date() <= this.survey.abilityVoteTo) {
      return true;
    }
    else if (new Date() >= this.survey.abilityVoteFrom) {
      return true;
    }
    return false;
  }

  abilityMultivote(): boolean {
    if (!this.survey.severalAnswer && this.isVote()) {
      return false;
    }
    else if (this.survey.severalAnswer) {
      return true;
    }
  }

  getDeadtimeTo(date: Date): string {
    return moment(date).toNow();
  }

  getDeadtimeFrom(date: Date): string {
    return moment(date).fromNow();
  }
}
