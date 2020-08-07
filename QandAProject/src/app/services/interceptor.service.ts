import { Injectable } from '@angular/core';
// import { IUserService } from "../interfaces/user-service.interface";
import { UserService } from './user-service.service';
import { serverAddress } from '../consts/server-address';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class InterceptorService {

  constructor(private userService: UserService, private http: HttpClient) { }

  public get(url: string): Observable<any>{
    const token = this.userService.getUser()?.authorizationToken;
    return this.http.get<any>(
      serverAddress + url,
      {
        headers: {
          AuthorizationToken: token ?? ''
        }
      });
  }

  public post(url: string, body: any): Observable<any>{
    const token = this.userService.getUser()?.authorizationToken;
    return this.http.post<any>(
      serverAddress + url,
      body,
      {
        headers: {
          AuthorizationToken: token ?? ''
        }
      });
  }

}
