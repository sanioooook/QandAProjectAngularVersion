import { Component } from '@angular/core';
import { SurveysService } from '../services/surveys.service';
import { Survey } from '../classes/survey';
import { Answer } from '../classes/answer';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-create-survey',
  templateUrl: './create-survey.component.html',
  styleUrls: ['./create-survey.component.css']
})
export class CreateSurveyComponent {

  constructor(private router: Router, private surveyService: SurveysService) {
    this.answers = new Array<Answer>();
  }

  questionSurvey: string;
  answers: Array<Answer>;
  newAnswer: string;
  addResponse: boolean;
  severalAnswer: boolean;

  CreateSurvey(): void {
    if (this.answers.length > 0 || this.questionSurvey === '') {
      const survey = new Survey();
      survey.id = 0;
      survey.answers = this.answers;
      survey.question = this.questionSurvey;
      survey.addResponse = this.addResponse;
      survey.severalAnswer = this.severalAnswer;
      this.surveyService.CreateSurvey(survey)
      .then(_ => this.router.navigate(['home']))
      .catch((Error: HttpErrorResponse) => window.alert(Error.error));
    } // отобразить ошибку что число ответов не может быть равным нулю
  }

  PushAnswer(): void {
    if (this.newAnswer !== '') {
      const answer = new Answer();
      answer.id = 0;
      answer.idSurvey = 0;
      answer.textAnswer = this.newAnswer;
      this.answers.push(answer);
      this.newAnswer = '';
    } // тоже отобразить ошибку что ответ не может быть пустой
  }

  showEditAnswerWindow(answer: Answer): void {
    const newAnswer = window.prompt('edit answer', answer.textAnswer);
    if (newAnswer !== null) {
      answer.textAnswer = newAnswer;
    }
  }
}
