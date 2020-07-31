import { Injectable } from "@angular/core";
import { CanActivate, CanDeactivate } from "@angular/router";
import { UserService } from '../services/user-service.service';
import { Observable } from "rxjs";

export interface CanComponentDeactivate {
  canDeactivate: () => Observable<boolean> | Promise<boolean> | boolean;
}

@Injectable({
  providedIn: 'root'
})

export class Authorized implements CanActivate, CanDeactivate<CanComponentDeactivate> {

  constructor(private userService: UserService) { }

  canActivate(): boolean {
    return this.userService.IsUserLogged();
  }

  canDeactivate():boolean {
    return !this.userService.IsUserLogged();
  }

}
