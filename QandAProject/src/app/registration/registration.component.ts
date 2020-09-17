import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../services/user-service.service';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.css']
})
export class RegistrationComponent {

  pattern = '';
  loginFormControl = new FormControl('', [Validators.required, Validators.email]);
  passwordFormControl =
    new FormControl('', [Validators.required, Validators.minLength(8)]);
  secondPasswordFormControl =
    new FormControl('', [
      Validators.required,
      Validators.minLength(8),
      Validators.pattern(this.pattern)
    ]);
  hidePassword = false;
  hideSecondPassword = false;

  constructor(private router: Router, private userService: UserService) { }

  getEmailErrorMessage(): string {
    if (this.loginFormControl.hasError('required')) {
      return 'You must enter a value';
    }
    if (this.loginFormControl.hasError('email')) {
      return 'Not a valid email';
    }
    return '';
  }

  getPasswordErrorMessage(): string {
    if (this.passwordFormControl.hasError('required')) {
      return 'You must enter a value';
    }
    if (this.passwordFormControl.hasError('minlength')) {
      return `Minimal length password is ${this.passwordFormControl.errors.minlength.requiredLength}`;
    }
    return '';
  }

  getSecondPasswordErrorMessage(): string {
    if (this.secondPasswordFormControl.hasError('required')) {
      return 'You must enter a value';
    }
    if (this.secondPasswordFormControl.hasError('minlength')) {
      return `Minimal length password is ${this.secondPasswordFormControl.errors.minlength.requiredLength}`;
    }
    if (this.secondPasswordFormControl.hasError('pattern')){
      return 'Passwords must be the equal';
    }
    return '';
  }

  registration(): void {
    if (this.loginFormControl.valid
      && this.passwordFormControl.valid
      && this.secondPasswordFormControl.valid) {
      this.userService.registration(this.loginFormControl.value,
        this.passwordFormControl.value)
        .then(_ => this.router.navigate(['home']))
        .catch((err: HttpErrorResponse) => console.log(err));
    }
  }

  updatePattern(): void {
    this.secondPasswordFormControl
    .setValidators(Validators.pattern(this.passwordFormControl.value));
  }
}
