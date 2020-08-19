import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { User } from '../classes/user';
import { serverAddress } from '../consts/server-address';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private http: HttpClient) {
    this.user = new User();
    this.user.authorizeToken = localStorage.getItem('user') ?? '';
  }

  private user: User;

  public getAuthorizationToken(): string {
    return this.user.authorizeToken;
  }

  public login(email: string, password: string): Promise<any> {
    return this.http.post<User>(
      serverAddress + 'user/login',
      {
        login: email,
        password
      }
    ).toPromise()
    .then(data =>
      this.setAuthorizationToken(data.authorizeToken)
    )
    .catch((Error: HttpErrorResponse) => {
      window.alert(Error.error);
    });
  }

  public registration(email: string, password: string): Promise<any> {
    return this.http.post<User>(
      serverAddress + 'user/registration',
      {
        login: email,
        password
      }
    ).toPromise()
      .then(data =>
        this.setAuthorizationToken(data.authorizeToken)
      )
      .catch((Error: HttpErrorResponse) => {
        window.alert(Error.error);
      });
  }

  public IsUserLogged(): boolean{
    return this.user.authorizeToken !== '';
  }

  public setAuthorizationToken(authorizeToken: string): void {
    this.user.authorizeToken = authorizeToken;
    localStorage.setItem('user', authorizeToken);
  }

  public logOut(): Promise<any> {
    return this.http.get<any>(
      serverAddress + 'user/logout',
      {
        headers: {
          AuthorizationToken: this.getAuthorizationToken()
        }
      }).toPromise()
      .then(_ => {
        this.user.authorizeToken = '';
        localStorage.removeItem('user');
      });
  }
}
