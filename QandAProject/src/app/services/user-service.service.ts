import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { User } from 'src/app/classes/user'
import { serverAddress } from '../consts/server-address'
import { Observable, Observer, Subscription } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private http: HttpClient) {
    this.user = new User;
    this.user.authorizationToken = localStorage.getItem("user") ?? '';
  }
  
  private user: User;

  public getUser(): User {
    return this.user;
  }

  public async login(email: string, password: string): Promise<any> {
    const data = await this.http.post<any>(
      serverAddress + 'user/login',
      {
        login: email,
        password
      }
    ).toPromise();
    return this.setAuthorizationToken(data.authorizeToken);
  }

  public async registration(email: string, password: string): Promise<any> {
    const data = await this.http.post<any>(
      serverAddress + 'user/registration',
      {
        login: email,
        password
      }
    )
      .toPromise();
    return this.setAuthorizationToken(data.authorizeToken);
  }

  public IsUserLogged():boolean{
    return this.user.authorizationToken != '';
  }

  public setAuthorizationToken(authorizeToken:string):void {
    this.user.authorizationToken = authorizeToken;
    localStorage.setItem("user", authorizeToken);
  }

  public logOut(): Promise<any> {
    return this.http.get<any>(
      serverAddress + 'user/logout',
      {
        headers: {
          'AuthorizationToken': this.user.authorizationToken
        }
      }).toPromise()
      .then(() => {
        this.user.authorizationToken = '';
        localStorage.removeItem("user");
      });
  }
}
