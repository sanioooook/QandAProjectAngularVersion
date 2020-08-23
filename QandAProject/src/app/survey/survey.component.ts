import { Component, OnInit } from '@angular/core';
import { Survey } from '../classes/survey';
import { ActivatedRoute } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { SurveysService } from '../services/surveys.service';
import { HttpErrorResponse } from '@angular/common/http';
import { Answer } from '../classes/answer';
import { Vote } from '../classes/vote';
import { UserForPublic } from '../classes/user-for-public';
import { InterceptorService } from '../services/interceptor.service';

@Component({
  selector: 'app-survey',
  templateUrl: './survey.component.html',
  styleUrls: ['./survey.component.css']
})
export class SurveyComponent implements OnInit {
  constructor(private route: ActivatedRoute,
              private surveyService: SurveysService,
              private interceptorService: InterceptorService) { }

  user: UserForPublic;
  newAnswer: string;
  survey: Survey;
  voteCountInSurvey: number;

  ngOnInit(): void {
    this.interceptorService.get('User')
      .subscribe((userForPublic: UserForPublic) => this.user = userForPublic);
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
      this.voteCountInSurvey += answer.votes.length;
      if (answer.votes.length > 0) {
        answer.isUserVote = false;
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

}
