import { Component } from '@angular/core';
import { UserService } from '../services/user-service.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, Validators } from '@angular/forms';

@Component({

  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {

  loginFormControl = new FormControl('', [Validators.required, Validators.email]);
  passwordFormControl = new FormControl('', [Validators.required]);
  hidePassword = false;

  constructor(private router: Router, private userService: UserService) { }

  getEmailErrorMessage(): string {
    if (this.loginFormControl.hasError('required')) {
      return 'You must enter a value';
    }
    return this.loginFormControl.hasError('email') ? 'Not a valid email' : '';
  }

  getPasswordErrorMessage(): string {
    return this.passwordFormControl.hasError('required') ? 'You must enter a value' : '';
  }

  login(): void {
    if (this.loginFormControl.valid && this.passwordFormControl.valid) {
      this.userService.login(this.loginFormControl.value, this.passwordFormControl.value)
        .then(() => this.router.navigate(['home']))
        .catch((err: HttpErrorResponse) => {
          console.log(err);
        });
    }
  }
}
