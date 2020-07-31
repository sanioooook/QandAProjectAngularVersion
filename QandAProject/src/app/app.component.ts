import { Component } from '@angular/core';
import { UserService } from './services/user-service.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  constructor(public userService: UserService,private _router: Router){

  }
  title = 'QandAProject';
  links = [
    {title: 'Login', link: 'login'},
    {title: 'Registration', link: 'registration'}
  ];

  navigateHome():void{
    this._router.navigate(['home']);
  }
  
  logOut(): void {
    this.userService.logOut().then(
      _ => this._router.navigate(['home']),
      (Error: HttpErrorResponse) =>
        window.alert(Error.error.error)
    );
  }
}
