import { Component, OnInit, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TdDialogService } from '@covalent/core/dialogs';
import { FormControl, Validators } from '@angular/forms';
import { MatDialog, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SurveysService } from '../services/surveys.service';
import { Survey } from '../classes/survey';
import { Answer } from '../classes/answer';

@Component({
  selector: 'app-create-survey',
  templateUrl: './create-survey.component.html',
  styleUrls: ['./create-survey.component.css']
})
export class CreateSurveyComponent implements OnInit {

  constructor(private router: Router,
              private surveyService: SurveysService,
              private dialogService: TdDialogService,
              public dialog: MatDialog) {
    this.survey = new Survey();
    this.survey.answers = new Array<Answer>();
    this.newAnswer = 'Yes';
    this.PushAnswer();
    this.survey.id = 0;
  }


  question = new FormControl([Validators.required]);
  survey: Survey;
  newAnswer = '';

  ngOnInit(): void {

  }

  getErrorMessage(): string {
    if (this.question.hasError('required')) {
      return 'You must enter a value';
    }
  }

  CreateSurvey(): void {
    if (this.survey.answers.length > 0 && this.survey.question) {
      this.surveyService.CreateSurvey(this.survey)
        .then(_ => this.router.navigate(['home']))
        .catch((Error: HttpErrorResponse) => console.log(Error.error));
    } // отобразить ошибку что число ответов не может быть равным нулю
  }

  PushAnswer(): void {
    if (this.newAnswer) {
      const answer = new Answer();
      answer.id = 0;
      answer.idSurvey = 0;
      answer.textAnswer = this.newAnswer;
      this.survey.answers.push(answer);
      this.newAnswer = '';
    } // тоже отобразить ошибку что ответ не может быть пустой
  }

  showEditAnswerWindow(answer: Answer): void {
    this.dialogService.openPrompt({
      title: 'Edit answer',
      message: 'Edit text answer',
      value: answer.textAnswer,
      cancelButton: 'Cancel',
      acceptButton: 'Ok',
    })
      .afterClosed()
      .subscribe(
        (editedAnswer: string) => {
          if (editedAnswer) {
            answer.textAnswer = editedAnswer;
          }
        });
  }

  openSettings(): void {
    this.dialog.open(SettingDialog, { data: this.survey });
  }
}

@Component({
  selector: 'app-setting-dialog',
  templateUrl: 'setting-dialog.html',
})
// tslint:disable-next-line: component-class-suffix
export class SettingDialog {
  constructor(@Inject(MAT_DIALOG_DATA) public data: Survey) { }

  now(): Date {
    return new Date();
  }

  minDateTo(): Date {
    if (this.data && this.data.abilityVoteFrom) {
      const dateTime = this.now();
      dateTime.setHours(this.data.abilityVoteFrom.getHours() + 1);
      return dateTime;
    }
    return this.now();
  }

  minDateFrom(): Date {
    const dateTime = this.now();
    dateTime.setMinutes(dateTime.getMinutes() + 5);
    dateTime.setSeconds(0, 0);
    return dateTime;
  }

}
