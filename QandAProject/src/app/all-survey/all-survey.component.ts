import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material/table';
import { PageEvent } from '@angular/material/paginator';
import { SurveysService } from '../services/surveys.service';
import { Survey } from '../classes/survey';
import { UserService } from '../services/user-service.service';
import { UserForPublic } from '../classes/user-for-public';
import { Pagination } from '../classes/pagination';
import { SurveySortBy } from '../classes/survey-sort-by.enum';
import { SortDirection } from '../classes/sort-direction.enum';
import { Sort } from '../classes/sort';
import { Filter } from '../classes/filter';

@Component({
  selector: 'app-all-survey',
  templateUrl: './all-survey.component.html',
  styleUrls: ['./all-survey.component.scss']
})

export class AllSurveyComponent implements OnInit {

  constructor(private surveysService: SurveysService,
              private userService: UserService) {
  }

  public surveyPagination: Pagination<Survey>;
  public dataSource: MatTableDataSource<Survey>;
  public filter = new Filter();
  private user: UserForPublic;
  private sortBy = SurveySortBy.Id;
  private sortDirection = SortDirection.Descending;

  ngOnInit(): void {
    this.user = new UserForPublic();
    this.userService.getUserLogin().then(login => this.user.login = login);
    this.filter.searchQuery = '';
    this.getSurveys();
  }

  getSurveys(): void {
    let paginator = new Pagination<Survey>();
    if (!this.surveyPagination) {
      paginator.pageNumber = 0;
      paginator.pageSize = 10;
    }
    else {
      paginator = this.surveyPagination;
    }
    const sort = new Sort<SurveySortBy>();
    sort.sortBy = this.sortBy;
    sort.sortDirection = this.sortDirection;
    this.surveysService.GetAllSurveys(paginator, sort, this.filter)
      .then(data => {
        this.surveyPagination = data;
        this.dataSource = new MatTableDataSource(this.surveyPagination.data);
      })
      .catch((Error: HttpErrorResponse) => console.log(Error.error));
  }

  sortData(sort): void {
    if (!sort.active || sort.direction === '') {
      return;
    }
    if (sort.direction === 'asc') {
      this.sortDirection = SortDirection.Ascending;
    }
    else {
      this.sortDirection = SortDirection.Descending;
    }
    switch (sort.active) {
      case 'Question':
        this.sortBy = SurveySortBy.Question;
        break;
      case 'NumberAnswers':
        this.sortBy = SurveySortBy.NumberAnswers;
        break;
      case 'NumberVotes':
        this.sortBy = SurveySortBy.NumberVotes;
        break;
      case 'PermissionEdit':
        this.sortBy = SurveySortBy.PermissionEdit;
        break;
      case 'TimeCreate':
        this.sortBy = SurveySortBy.TimeCreate;
        break;
    }
    this.getSurveys();
  }

  isUserOwner(survey: Survey): boolean {
    return survey.user.login === this.user.login;
  }

  getNumberVotes(survey: Survey): number {
    let numberVotes = 0;
    survey.answers.forEach(answer => numberVotes += answer.votes.length);
    return numberVotes;
  }

  requestNewPage(pageEvent: PageEvent): void {
    this.surveyPagination.pageNumber = pageEvent.pageIndex;
    this.surveyPagination.pageSize = pageEvent.pageSize;
    this.getSurveys();
  }
}
