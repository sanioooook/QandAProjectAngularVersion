import { Injectable } from '@angular/core';
import { InterceptorService } from './interceptor.service';
import { Survey } from '../classes/survey';
import { Vote } from '../classes/vote';
import { Answer } from '../classes/answer';

@Injectable({
  providedIn: 'root'
})
export class SurveysService {

  constructor(private interceptor: InterceptorService) { }

  CreateSurvey(survey: Survey): Promise<any> {
    return this.interceptor.post('survey/create', survey).toPromise();
  }

  GetAllSurveys(): Promise<Survey[]> {
    return this.interceptor.get('survey').toPromise();
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

  GetUserSurveys(): Promise<Survey[]> {
    return this.interceptor.get('survey/userSurveys').toPromise();
  }

  GetSurveyById(id: number): Promise<Survey> {
    return this.interceptor.get(`survey/${id}`).toPromise();
  }

  EditSurvey(survey: Survey): Promise<any> {
    return this.interceptor.post('survey/edit', survey).toPromise();
  }

  DeleteSurvey(id: number): Promise<any> {
    return this.interceptor.delete(`survey/${id}`).toPromise();
  }
}
