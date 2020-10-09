import { Injectable } from '@angular/core';
import { InterceptorService } from './interceptor.service';
import { Survey } from '../classes/survey';
import { Vote } from '../classes/vote';
import { Answer } from '../classes/answer';
import { Pagination } from '../classes/pagination';
import { HttpParams } from '@angular/common/http';
import { Sort } from '../classes/sort';
import { SurveySortBy } from '../classes/survey-sort-by.enum';
import { Filter } from '../classes/filter';
import { map, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import * as momentTimezone from 'moment-timezone';
import { UserService } from './user-service.service';
import { User } from '../classes/user';

@Injectable({
  providedIn: 'root'
})
export class SurveysService {

  constructor(private interceptor: InterceptorService,
              private userService: UserService) {
    this.userService.getUserLogin()
      .then((userLogin: string) => this.user.login = userLogin);
  }

  user = new User();

  CreateSurvey(survey: Survey): Promise<any> {
    return this.interceptor.post('survey/create', survey).toPromise();
  }

  GetAllSurveys(surveyPagination: Pagination<Survey>,
                surveySort: Sort<SurveySortBy>,
                filter: Filter): Promise<Pagination<Survey>> {
    return this.interceptor.get(
      'survey', new HttpParams()
        .set('pageNumber', surveyPagination.pageNumber.toString())
        .set('pageSize', surveyPagination.pageSize.toString())
        .set('sortBy', surveySort.sortBy.toString())
        .set('sortDirection', surveySort.sortDirection.toString())
        .set('filter', filter.searchQuery)
    ).pipe(map((data: Pagination<Survey>) => {
      data.data.forEach(survey => {
        this.processingSurvey(survey);
      });
      return data;
    }), catchError(err => throwError(err))).toPromise();
  }

  Vote(votes: Vote[]): Promise<any> {
    return this.interceptor.post('vote', votes).toPromise();
  }

  AddNewAnswer(answer: Answer): Promise<any> {
    return this.interceptor.post('answer', answer).toPromise();
  }

  EditAnswer(answer: Answer): Promise<any> {
    return this.interceptor.post('answer/editAnswer', answer).toPromise();
  }

  DeleteAnswer(id: number): Promise<any> {
    return this.interceptor.delete(`answer/${id}`).toPromise();
  }

  GetSurveyById(id: number): Promise<Survey> {
    return this.interceptor.get(`survey/${id}`)
      .pipe(map((survey: Survey) => {
        this.processingSurvey(survey);
        return survey;
      }), catchError(err => throwError(err))).toPromise();
  }

  EditSurvey(survey: Survey): Promise<any> {
    return this.interceptor.post('survey/edit', survey).toPromise();
  }

  DeleteSurvey(id: number): Promise<any> {
    return this.interceptor.delete(`survey/${id}`).toPromise();
  }

  private processingSurvey(survey: Survey): void {
    survey.answers.forEach(answer => {
      answer.isUserVote = false;
      if (answer.votes.length > 0) {
        answer.votes.forEach(vote => {
          if (!answer.isUserVote && vote.voter === this.user.login) {
            answer.isUserVote = true;
          }
        });
      }
    });
    survey.abilityVoteFrom = momentTimezone.utc(survey.abilityVoteFrom).local().toDate();
    survey.abilityVoteTo = survey.abilityVoteTo
      ? momentTimezone.utc(survey.abilityVoteTo).local().toDate()
      : null;
  }
}
