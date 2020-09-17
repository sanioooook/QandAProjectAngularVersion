import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { TdDialogService } from '@covalent/core/dialogs';
import { User } from '../classes/user';
import { serverAddress } from '../consts/server-address';
import { UserForPublic } from '../classes/user-for-public';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  constructor(private http: HttpClient,
              public dialogService: TdDialogService) {
    this.user = new User();
    this.user.authorizeToken = localStorage.getItem('user') ?? '';
  }

  private user: User;

  public getAuthorizationToken(): string {
    return this.user.authorizeToken;
  }

  public getUserLogin(): Promise<string> {
    if (this.user.login) {
      return new Promise<string>(resolve => resolve(this.user.login));
    }
    else if (this.user.authorizeToken) {
      return new Promise<string>((resolve, reject) => {
        this.getUserLoginFromServer().subscribe((user: UserForPublic) => {
          this.setUserLogin(user.login);
          resolve(user.login);
        },
          error => reject(error));
      });
    }
    return new Promise((_, reject) => reject('not have token'));
  }

  public login(email: string, password: string): Promise<any> {
    return this.http.post<User>(
      serverAddress + 'user/login',
      {
        login: email,
        password
      }
    ).pipe(map(data => data), catchError(err => this.catchCallback(err))).toPromise()
      .then(data => {
        this.setAuthorizationToken(data.authorizeToken);
        this.setUserLogin(data.login);
      }
      );
  }

  public registration(email: string, password: string): Promise<any> {
    return this.http.post<User>(
      serverAddress + 'user/registration',
      {
        login: email,
        password
      }
    ).pipe(map(data => data), catchError(err => this.catchCallback(err))).toPromise()
      .then(data => {
        this.setAuthorizationToken(data.authorizeToken);
        this.setUserLogin(data.login);
      }
      );
  }

  public IsUserLogged(): boolean {
    return this.user.authorizeToken !== '';
  }

  private setAuthorizationToken(authorizeToken: string): void {
    this.user.authorizeToken = authorizeToken;
    localStorage.setItem('user', authorizeToken);
  }

  private setUserLogin(login: string): void {
    this.user.login = login;
  }

  public logOut(): Promise<any> {
    return this.http.get<any>(
      serverAddress + 'user/logout',
      {
        headers: {
          AuthorizationToken: this.getAuthorizationToken()
        }
      }).pipe(map(data => data), catchError(err => this.catchCallback(err))).toPromise()
      .then(_ => {
        this.user.authorizeToken = '';
        localStorage.removeItem('user');
      });
  }

  private getUserLoginFromServer(): Observable<UserForPublic>
  {
    return this.http.get<UserForPublic>(
      serverAddress + 'user',
      {
        headers: {
          AuthorizationToken: this.user.authorizeToken
        }
      });
  }

  private catchCallback(err: HttpErrorResponse): Observable<any> {
    for (const propertyOnError in err.error) {
      if (Object.prototype.hasOwnProperty.call(err.error, propertyOnError)) {
        const element = err.error[propertyOnError];
        element.forEach(elementForeach => {
          this.openAlert(`${propertyOnError}: ${elementForeach}`);
        });
      }
    }
    return throwError(err);
  }

  private openAlert(message: string): void {
    this.dialogService.openAlert({
      message,
      title: 'Alert',
      closeButton: 'Close',
    });
  }
}
