import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { SurveysService } from '../services/surveys.service';
import { Survey } from '../classes/survey';
import { UserService } from '../services/user-service.service';
import { UserForPublic } from '../classes/user-for-public';
import { Pagination } from '../classes/pagination';

@Component({
  selector: 'app-all-survey',
  templateUrl: './all-survey.component.html',
  styleUrls: ['./all-survey.component.css']
})

export class AllSurveyComponent implements OnInit {

  constructor(private surveysService: SurveysService,
              private userService: UserService) {
  }

  public surveys: Pagination<Survey>;
  public dataSource: MatTableDataSource<Survey>;
  private user: UserForPublic;


  ngOnInit(): void {
    this.user = new UserForPublic();
    this.userService.getUserLogin().then(login => this.user.login = login);
    this.getSurveys();
  }

  getSurveys(surveyPagination: Pagination<Survey> = null): void {
    let paginator = new Pagination<Survey>();
    if (!surveyPagination) {
      paginator.pageNumber = 0;
      paginator.pageSize = 10;
    }
    else {
      paginator = surveyPagination;
    }
    this.surveysService.GetAllSurveys(paginator)
      .then(data => {
        this.surveys = data;
        this.dataSource = new MatTableDataSource(this.surveys.data);
      })
      .catch((Error: HttpErrorResponse) => console.log(Error.error));
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  sortData(sort: Sort): void {
    const data = this.surveys.data.slice();
    if (!sort.active || sort.direction === '') {
      this.dataSource = new MatTableDataSource(data);
      return;
    }
    const isAsc = sort.direction === 'asc';
    this.dataSource = new MatTableDataSource(data.sort((a, b) => {
      switch (sort.active) {
        case 'question': return this.compare(a.question, b.question, isAsc);
        case 'numAnswers': return this.compare(a.answers.length, b.answers.length, isAsc);
        case 'timeCreateSurvey': return this.compare(a.timeCreate, b.timeCreate, isAsc);
        case 'numberVotes':
          return this.compare(this.getNumberVotes(a), this.getNumberVotes(b), isAsc);
        case 'canEditSurvey':
          return this.compare(+this.isUserOwner(a), +this.isUserOwner(b), isAsc);
        default: return 0;
      }
    }));
  }

  private compare(a: number | string, b: number | string, isAsc: boolean): number {
    return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
  }

  getNumberVotes(survey: Survey): number {
    let numberVotes = 0;
    survey.answers.forEach(answer => numberVotes += answer.votes.length);
    return numberVotes;
  }

  isUserOwner(survey: Survey): boolean {
    return survey.user.login === this.user.login;
  }

  requestNewPage(pageEvent: PageEvent): void {
    const paginator = new Pagination<Survey>();
    paginator.pageNumber = pageEvent.pageIndex;
    paginator.pageSize = pageEvent.pageSize;
    this.getSurveys(paginator);
  }
}
