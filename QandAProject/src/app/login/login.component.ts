import { Component, OnInit } from '@angular/core';
import { UserService } from '../services/user-service.service'
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

  constructor(private _router: Router, private userService: UserService) { }

  ngOnInit(): void {
  }

  Login(): void {
    this.userService.login(this.login, this.password).then(() => 
      this._router.navigate(['home'])
    ,
    (Error: HttpErrorResponse)=> {
      window.alert(Error.error.error);
    });
  }

  // canDeactivate() {
  //   return window.confirm('Do you really want to exit?');
  // }
}
