import { Injectable } from '@angular/core';
import { TdDialogService } from '@covalent/core/dialogs';
import { UserService } from './user-service.service';
import { serverAddress } from '../consts/server-address';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { map, catchError} from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})

export class InterceptorService {

  constructor(private userService: UserService,
              private http: HttpClient,
              private dialogService: TdDialogService) { }

  public get(url: string, httpParams: HttpParams = null): Observable<any> {
    const token = this.userService.getAuthorizationToken();
    return this.http.get<any>(
      serverAddress + url,
      {
        headers: {
          AuthorizationToken: token
        },
        params: httpParams
      }).pipe(map(data => data),
        catchError(err => this.catchCallback(err)));
  }

  public post(url: string, body: any, httpParams: HttpParams = null): Observable<any>{
    const token = this.userService.getAuthorizationToken();
    return this.http.post<any>(
      serverAddress + url,
      body,
      {
        headers: {
          AuthorizationToken: token
        },
        params: httpParams
      }).pipe(map(data => data), catchError(err => this.catchCallback(err)));
  }

  public delete(url: string): Observable<any> {
    const token = this.userService.getAuthorizationToken();
    return this.http.delete<any>(
      serverAddress + url,
      {
        headers: {
          AuthorizationToken: token
        }
      }).pipe(map(data => data), catchError(err => this.catchCallback(err)));
  }

  private catchCallback(err: HttpErrorResponse): Observable<any> {
    for (const property in err.error) {
      if (Object.prototype.hasOwnProperty.call(err.error, property)) {
        const element = err.error[property];
        element.forEach(elementForeach => {
          this.openAlert(`${property}: ${elementForeach}`);
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
