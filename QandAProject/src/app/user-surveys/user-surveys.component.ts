import { Component, OnInit } from '@angular/core';
import { SurveysService } from '../services/surveys.service';
import { Survey } from '../classes/survey';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-user-surveys',
  templateUrl: './user-surveys.component.html',
  styleUrls: ['./user-surveys.component.css']
})
export class UserSurveysComponent implements OnInit {

  constructor(private surveysService: SurveysService) {
  }

  public surveys: Survey[];

  ngOnInit(): void {
    this.getSurveys();
  }

  getSurveys(): void {
    this.surveysService.GetUserSurveys()
      .then(data => {
        this.surveys = data;
      })
      .catch((Error: HttpErrorResponse) => window.alert(Error.error));
  }

  getNumberVotes(survey: Survey): number {
    let numberVotes = 0;
    survey.answers.forEach(answer => numberVotes += answer.votes.length);
    return numberVotes;
  }

}
