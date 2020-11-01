import { Component, OnInit } from '@angular/core';
import { Survey } from '../classes/survey';
import { ActivatedRoute, Router } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { SurveysService } from '../services/surveys.service';
import { HttpErrorResponse } from '@angular/common/http';
import { Answer } from '../classes/answer';
import { TdDialogService } from '@covalent/core/dialogs';
import { FormControl, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { SettingDialog } from '../create-survey/create-survey.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UserService } from '../services/user-service.service';

@Component({
  selector: 'app-edit-survey',
  templateUrl: './edit-survey.component.html',
  styleUrls: ['./edit-survey.component.scss']
})
export class EditSurveyComponent implements OnInit {
  constructor(private route: ActivatedRoute,
              private surveyService: SurveysService,
              private router: Router,
              private dialogService: TdDialogService,
              private dialog: MatDialog,
              private snackBar: MatSnackBar,
              private userService: UserService) {
    this.question.registerOnChange(() => this.survey.question = this.question.value);
  }

  newAnswer = '';
  survey: Survey;
  question = new FormControl('', [Validators.required]);

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => params.getAll('id')))
      .subscribe(id => this.setSurvey(+id));
  }

  getErrorMessage(): string {
    if (this.question.hasError('required')) {
      return 'You must enter a value';
    }
  }

  setSurvey(id: number): void {
    this.surveyService.GetSurveyById(id)
      .then(survey => {
        this.userService.getUserLogin().then(login => {
          if (survey.user.login === login) {
            this.survey = survey;
            this.question.setValue(survey.question);
          }
        });
      })
      .catch((Error: HttpErrorResponse) => console.log(Error.error));
  }

  addAnswer(): void {
    if (this.newAnswer) {
      this.surveyService.AddNewAnswer(new Answer(this.survey.id, this.newAnswer))
        .then((newAnswer: Answer) => {
          this.openSnackBar('Answer successfully added', 'OK');
          this.survey.answers.push(newAnswer);
          this.newAnswer = '';
        })
        .catch((Error: HttpErrorResponse) => console.log(Error.error));
    }
  }

  private editAnswer(answer: Answer): void {
    this.surveyService.EditAnswer(answer)
    .then(_ => this.openSnackBar('Answer successfully edited', 'OK'))
      .catch((Error: HttpErrorResponse) => console.log(Error.error));
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
        (newAnswer: string) => {
          if (newAnswer) {
            answer.textAnswer = newAnswer;
            this.editAnswer(answer);
          }
        });
  }

  deleteAnswer(answer: Answer): Promise<any> {
    return this.surveyService.DeleteAnswer(answer.id)
      .then(_ => this.openSnackBar('Answer successfully deleted', 'OK'))
      .catch(_ => this.ngOnInit());
  }

  saveSurvey(): void {
    this.surveyService.EditSurvey(this.survey)
    .then(_ => this.openSnackBar('Survey successfully saved', 'OK'))
      .catch(_ => this.ngOnInit());
  }

  deleteSurvey(): void {
    this.surveyService.DeleteSurvey(this.survey.id)
      .then(_ => {
        this.openSnackBar('Survey successfully deleted', 'OK');
        this.router.navigate(['home']);
      })
      .catch((Error: HttpErrorResponse) => console.log(Error.error));
  }

  openSettings(): void {
    this.dialog.open(SettingDialog, { data: this.survey });
  }

  openSnackBar(message: string, action: string): void {
    this.snackBar.open(message, action, {
      duration: 2000,
    });
  }
}
