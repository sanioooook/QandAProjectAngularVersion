import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../services/user-service.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.css']
})
export class RegistrationComponent {

  login = '';
  password = '';
  secondPassword = '';
  pattern = '';
  constructor(private router: Router, private userService: UserService) { }

  Registration(): void {
    this.userService.registration(this.login, this.password)
      .then(_ => this.router.navigate(['home']))
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
