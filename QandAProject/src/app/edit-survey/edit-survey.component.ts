import { Component, OnInit } from '@angular/core';
import { Survey } from '../classes/survey';
import { ActivatedRoute, Router } from '@angular/router';
import { switchMap } from 'rxjs/operators';
import { SurveysService } from '../services/surveys.service';
import { HttpErrorResponse } from '@angular/common/http';
import { Answer } from '../classes/answer';
import { UserForPublic } from '../classes/user-for-public';
import { InterceptorService } from '../services/interceptor.service';
import { TdDialogService } from '@covalent/core/dialogs';

@Component({
  selector: 'app-edit-survey',
  templateUrl: './edit-survey.component.html',
  styleUrls: ['./edit-survey.component.css']
})
export class EditSurveyComponent implements OnInit {
  constructor(private route: ActivatedRoute,
              private surveyService: SurveysService,
              private interceptorService: InterceptorService,
              private router: Router,
              private dialogService: TdDialogService) { }

  user: UserForPublic;
  newAnswer: string;
  survey: Survey;
  editQuestionMode: boolean;

  ngOnInit(): void {
    this.interceptorService.get('User')
      .subscribe((userForPublic: UserForPublic) => this.user = userForPublic);
    this.route.paramMap.pipe(
      switchMap(params => params.getAll('id')))
      .subscribe(id => this.setSurvey(+id));
    this.editQuestionMode = false;
    this.newAnswer = '';
  }

  setSurvey(id: number): void {
    this.surveyService.GetSurveyById(id)
      .then(survey => {
        if (survey.user.login === this.user.login) {
          this.survey = survey;
        }
        else {
          this.survey = null;
        }
      })
      .catch((Error: HttpErrorResponse) => console.log(Error.error));
  }

  addAnswer(): void {
    if (this.newAnswer.trim() !== '') {
      const answer = new Answer();
      answer.textAnswer = this.newAnswer;
      answer.idSurvey = this.survey.id;
      answer.id = 0;
      this.surveyService.AddNewAnswer(answer)
        .then((newAnswer: Answer) => {
          this.survey.answers.push(newAnswer);
          this.newAnswer = '';
        })
        .catch((Error: HttpErrorResponse) => console.log(Error.error));
    }
  }

  editAnswer(answer: Answer): void {
    this.surveyService.EditAnswer(answer)
    .then()
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
      .catch(_ =>  this.ngOnInit());
  }

  saveSurvey(): void {
    this.surveyService.EditSurvey(this.survey)
      .catch(_ => this.ngOnInit());
  }

  deleteSurvey(): void {
    this.surveyService.DeleteSurvey(this.survey.id)
    .then(_ => this.router.navigate(['home']))
    .catch((Error: HttpErrorResponse) => console.log(Error.error));
  }
}
