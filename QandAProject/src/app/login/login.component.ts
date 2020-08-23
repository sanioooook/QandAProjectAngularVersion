import { Component } from '@angular/core';
import { UserService } from '../services/user-service.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({

  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  login = '';
  password = '';

  constructor(private router: Router, private userService: UserService) { }

  Login(): void {
    this.userService.login(this.login, this.password)
    .then(() => this.router.navigate(['home']))
    .catch((err: HttpErrorResponse) => {
      for (const property in err.error.errors) {
        if (Object.prototype.hasOwnProperty.call(err.error.errors, property)) {
          const element = err.error.errors[property];
          element.forEach((elementForeach: any) => {
            window.alert(`${property}: ${elementForeach}`);
          });
        }
      }
    });
  }
}
