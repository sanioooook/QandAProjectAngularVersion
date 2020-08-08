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

  GetAllSurveys(): Promise<Survey[]> {
    return this.interceptor.get('survey').toPromise();
  }

  Vote(vote: Vote): Promise<any> {
    return this.interceptor.post('vote', vote).toPromise();
  }

  AddNewAnswer(answer: Answer): Promise<any> {
    return this.interceptor.post('answer', answer).toPromise();
  }

  GetUserSurveys(): Promise<Survey[]> {
    return this.interceptor.get('survey/userSurveys').toPromise();
  }

}
