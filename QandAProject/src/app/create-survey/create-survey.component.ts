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
  styleUrls: ['./create-survey.component.scss']
})
export class CreateSurveyComponent implements OnInit {

  constructor(private router: Router,
              private surveyService: SurveysService,
              private dialogService: TdDialogService,
              public dialog: MatDialog) {
    this.survey = new Survey('', new Array<Answer>(), false, null, 1, 1, null);
    this.newAnswer = 'Yes';
    this.PushAnswer();
  }


  question = new FormControl('', [Validators.required]);
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
    if (this.survey.answers.length > 0 && this.question.valid) {
      this.survey.question = this.question.value;
      this.surveyService.CreateSurvey(this.survey)
        .then(_ => this.router.navigate(['home']))
        .catch((Error: HttpErrorResponse) => console.log(Error.error));
    } // отобразить ошибку что число ответов не может быть равным нулю
  }

  PushAnswer(): void {
    if (this.newAnswer) {
      this.survey.answers.push(new Answer(0, this.newAnswer));
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
  constructor(@Inject(MAT_DIALOG_DATA) public data: Survey) {
    this.minNumberVoteFormControl.registerOnChange(() => this.updateMinNumberVoteFormControl());
    this.maxNumberVoteFormControl.registerOnChange(() => this.updateMaxNumberVoteFormControl());
  }
  minNumberVoteFormControl = new FormControl(this.data.minCountVotes || 1,
    [
      Validators.required,
      Validators.min(1),
      Validators.max(this.data.answers.length)
    ]);

  maxNumberVoteFormControl = new FormControl(this.data.maxCountVotes,
    [
      Validators.min(this.minNumberVoteFormControl.value),
      Validators.max(this.data.answers.length)
    ]);

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

  updateMinNumberVoteFormControl(): void {
    if (this.minNumberVoteFormControl.valid) {
      this.data.minCountVotes = this.minNumberVoteFormControl.value;
      this.maxNumberVoteFormControl
        .setValidators(Validators.min(this.minNumberVoteFormControl.value));
    }
  }

  updateMaxNumberVoteFormControl(): void {
    if (this.maxNumberVoteFormControl.valid) {
      this.data.maxCountVotes = this.maxNumberVoteFormControl.value;
    }
  }

  getErrorMinNumberVote(): string{
    if (this.minNumberVoteFormControl.hasError('required')){
      return 'The minimum number of votes can\'t be empty';
    }
    if (this.minNumberVoteFormControl.hasError('min')){
      return 'The minimum number of votes can\'t be less than 1';
    }
    if (this.minNumberVoteFormControl.hasError('max')){
      return `The minimum number of votes can't be more than the number of answers (${this.data.answers.length})`;
    }
  }

  getErrorMaxNumberVote(): string{
    if (this.maxNumberVoteFormControl.hasError('min')){
      return `The maximum number of votes cannot exceed the minimum number of votes(${this.minNumberVoteFormControl.value})`;
    }
    if (this.maxNumberVoteFormControl.hasError('max')){
      return `The maximum number of votes can't be more than the number of answers (${this.data.answers.length})`;
    }
  }
}
