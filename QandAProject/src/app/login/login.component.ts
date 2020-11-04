import { Component } from '@angular/core';
import { UserService } from '../services/user-service.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, Validators } from '@angular/forms';
import { TranslocoService } from '@ngneat/transloco';
import { Observable } from 'rxjs';

@Component({

  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {

  loginFormControl = new FormControl('', [Validators.required, Validators.email]);
  passwordFormControl = new FormControl('', [Validators.required]);
  hidePassword = false;

  constructor(private router: Router,
              private translocoService: TranslocoService,
              private userService: UserService) { }

  getEmailErrorMessage(): Observable<string> {
    if (this.loginFormControl.hasError('required')) {
      return this.translocoService.selectTranslate('loginComponent.enterValue');
    }
    if (this.loginFormControl.hasError('email')) {
      return this.translocoService.selectTranslate('loginComponent.notValidEmail');
    }
    return new Observable<string>(observer => observer.next(''));
  }

  getPasswordErrorMessage(): Observable<string> {
    if (this.passwordFormControl.hasError('required'))
    {
      return this.translocoService.selectTranslate('loginComponent.enterValue');
    }
    return new Observable<string>(observer => observer.next(''));
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
