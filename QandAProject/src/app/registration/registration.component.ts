import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../services/user-service.service';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, Validators } from '@angular/forms';
import { TranslocoService } from '@ngneat/transloco';
import { Observable } from 'rxjs';


@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.scss']
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

  constructor(private router: Router,
              private translocoService: TranslocoService,
              private userService: UserService) { }

  getEmailErrorMessage(): Observable<string> {
    if (this.loginFormControl.hasError('required')) {
      return this.translocoService.selectTranslate('registrationComponent.enterValue');
    }
    if (this.loginFormControl.hasError('email')) {
      return this.translocoService.selectTranslate('registrationComponent.notValidEmail');
    }
    return new Observable<string>(observer => observer.next(''));
  }

  getPasswordErrorMessage(): Observable<string> {
    if (this.passwordFormControl.hasError('required')) {
      return this.translocoService.selectTranslate('registrationComponent.enterValue');
    }
    if (this.passwordFormControl.hasError('minlength')) {
      return this.translocoService.selectTranslate('registrationComponent.minimalLengthPassword',
        { minlength: this.passwordFormControl.errors.minlength.requiredLength });
    }
    return new Observable<string>(observer => observer.next(''));
  }

  getSecondPasswordErrorMessage(): Observable<string> {
    if (this.secondPasswordFormControl.hasError('required')) {
      return this.translocoService.selectTranslate('registrationComponent.enterValue');
    }
    if (this.secondPasswordFormControl.hasError('minlength')) {
      return this.translocoService.selectTranslate('registrationComponent.minimalLengthPassword',
        { minlength: this.secondPasswordFormControl.errors.minlength.requiredLength });
    }
    if (this.secondPasswordFormControl.hasError('pattern')){
      return this.translocoService.selectTranslate('registrationComponent.passwordsMustEqual');
    }
    return new Observable<string>(observer => observer.next(''));
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
