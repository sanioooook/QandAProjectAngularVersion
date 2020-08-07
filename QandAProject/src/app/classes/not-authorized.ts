import { Injectable } from '@angular/core';
import { CanDeactivate, CanActivate } from '@angular/router';
import { UserService } from '../services/user-service.service';
import { Observable } from 'rxjs';

export interface CanComponentDeactivate {
  canDeactivate: () => Observable<boolean> | Promise<boolean> | boolean;
}

@Injectable({
  providedIn: 'root'
})

export class NotAuthorized implements CanDeactivate<CanComponentDeactivate>, CanActivate{

  constructor(private userService: UserService) { }

  canActivate(): boolean {
    return !this.userService.IsUserLogged();
  }

  canDeactivate(): boolean {
    return this.userService.IsUserLogged();
  }

}
