import { Component, OnInit } from '@angular/core';
import { SurveysService } from '../services/surveys.service';
import { Survey } from '../classes/survey';
import { HttpErrorResponse } from '@angular/common/http';


@Component({
  selector: 'app-user-vote-surveys',
  templateUrl: './user-vote-surveys.component.html',
  styleUrls: ['./user-vote-surveys.component.css']
})
export class UserVoteSurveysComponent implements OnInit {


  constructor(private surveysService: SurveysService) {
  }

  public surveys: Survey[];

  ngOnInit(): void {
    this.getSurveys();
  }

  getSurveys(): void {
    this.surveysService.GetAllSurveys()
      .then(data => {
        this.surveys = data;
      })
      .catch((Error: HttpErrorResponse) => console.log(Error.error));
  }

  getNumberVotes(survey: Survey): number {
    let numberVotes = 0;
    survey.answers.forEach(answer => numberVotes += answer.votes.length);
    return numberVotes;
  }

}
