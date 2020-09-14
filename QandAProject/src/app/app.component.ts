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
  constructor(public userService: UserService, private router: Router){
  }
  title = 'QandAProject';

  logOut(): void {
    this.userService.logOut().then(
      _ => this.router.navigate(['home']),
      (Error: HttpErrorResponse) =>
        console.log(Error.error.error)
    );
  }
}
