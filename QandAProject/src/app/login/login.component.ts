import { Component, OnInit } from '@angular/core';
import { UserService } from '../services/user-service.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({

  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  login = '';
  password = '';

  constructor(private router: Router, private userService: UserService) { }

  ngOnInit(): void {
  }

  Login(): void {
    this.userService.login(this.login, this.password).then(() =>
      this.router.navigate(['home'])
    ,
    (Error: HttpErrorResponse) => {
      if (Error.error.errors !== undefined) {
        const objError = Error.error.errors;
        if (objError.Password !== undefined) {
          for (const error of objError.Password) {
            window.alert(error);
          }
        }
        if (objError.Login !== undefined) {
          for (const error of objError.Login) {
            window.alert(error);
          }
        }
      } else {
      window.alert(Error.error);
      }
    });
  }

  // canDeactivate() {
  //   return window.confirm('Do you really want to exit?');
  // }
}
