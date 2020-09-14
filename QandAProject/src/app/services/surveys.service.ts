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

@Injectable({
  providedIn: 'root'
})
export class SurveysService {

  constructor(private interceptor: InterceptorService) { }

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
        survey.abilityVoteFrom = momentTimezone.utc(survey.abilityVoteFrom).local().toDate();
        survey.abilityVoteTo = momentTimezone.utc(survey.abilityVoteTo).local().toDate();
      });
      return data;
    }), catchError(err => throwError(err))).toPromise();
  }

  Vote(vote: Vote): Promise<any> {
    return this.interceptor.post('vote', vote).toPromise();
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
        survey.abilityVoteFrom = new Date(survey.abilityVoteFrom + '');
        survey.abilityVoteTo = survey.abilityVoteTo ? new Date(survey.abilityVoteTo + '') : null;
        return survey;
      }), catchError(err => throwError(err))).toPromise();
  }

  EditSurvey(survey: Survey): Promise<any> {
    return this.interceptor.post('survey/edit', survey).toPromise();
  }

  DeleteSurvey(id: number): Promise<any> {
    return this.interceptor.delete(`survey/${id}`).toPromise();
  }
}
