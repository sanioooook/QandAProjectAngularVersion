import { Component, OnInit, OnChanges } from '@angular/core';
import { SurveysService } from '../services/surveys.service';
import { Survey } from '../classes/survey';
import { HttpErrorResponse } from '@angular/common/http';
import { UserForPublic } from '../classes/user-for-public';
import { InterceptorService } from '../services/interceptor.service';
import { Answer } from '../classes/answer';
import { Vote } from '../classes/vote';

@Component({
  selector: 'app-all-survey',
  templateUrl: './all-survey.component.html',
  styleUrls: ['./all-survey.component.css']
})
export class AllSurveyComponent implements OnInit {

  constructor(private surveysService: SurveysService, private interceptorService: InterceptorService) {
    this.mapNewAnswersInSurveys = new Map<number, string>();
  }

  public surveys: Survey[];
  public mapNewAnswersInSurveys: Map<number, string>;
  private user: UserForPublic;

  ngOnInit(): void {
    this.interceptorService.get('User')
      .subscribe((userForPublic: UserForPublic) => this.user = userForPublic);
    this.getSurveys();
  }

  getSurveys(): void {
    this.surveysService.GetAllSurveys()
      .then(data => {
        this.surveys = data;
        this.processingSurveys();
        this.fillMapNewAnswersInSurveys();
      })
      .catch((Error: HttpErrorResponse) => window.alert(Error.error));
  }

  processingSurveys(): void {
    this.surveys.forEach(survey => {
      survey.answers.forEach(answer => {
        if (answer.votes.length > 0) {
          answer.isUserVote = false;
          answer.votes.forEach(voteInAnswer => {
            if (!answer.isUserVote && voteInAnswer.voter === this.user.login) {
              answer.isUserVote = true;
            }
          });
        }
      });
    });
  }

  isVote(survey: Survey): boolean {
    for (const answer of survey.answers) {
      if (answer.isUserVote) {
        return true;
      }
    }
    return false;
  }

  fillMapNewAnswersInSurveys(): void {
    this.surveys.forEach(survey => {
      if (survey.addResponse) {
        this.mapNewAnswersInSurveys.set(survey.id, '');
      }
    });
  }

  async addAnswer(surveyId: number): Promise<any> {
    const newAnswer = this.mapNewAnswersInSurveys.get(surveyId);
    const answer = new Answer();
    answer.textAnswer = newAnswer;
    answer.idSurvey = surveyId;
    answer.id = 0;
    try {
      await this.surveysService.AddNewAnswer(answer);
      this.surveys.find(survey => survey.id === surveyId).answers.push(answer);
      this.mapNewAnswersInSurveys.set(surveyId, '');
    }
    catch (Error) {
      return window.alert(Error.error);
    }
  }

  vote(answerId: number): void {
    const vote = new Vote();
    vote.id = 0;
    vote.idAnswer = answerId;
    vote.voter = this.user.login;
    this.surveysService.Vote(vote)
      .then(
        () => this.surveys.forEach(
          survey => survey.answers.forEach(
            answer => {
              if (answer.id === answerId) {
                answer.isUserVote = true;
              }
            }
          )
        )
      )
      .catch((Error: HttpErrorResponse) => {
        window.alert(Error.error);
        this.surveys.forEach(
          survey => survey.answers.forEach(
            answer => {
              if (answer.id === answerId) {
                answer.isUserVote = false;
              }
            }
          )
        );
      });
  }
}
