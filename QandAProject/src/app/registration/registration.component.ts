import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../services/user-service.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.css']
})
export class RegistrationComponent implements OnInit {

  login:string = '';
  password:string = '';
  secondPassword:string = '';
  pattern:string = ''
  constructor(private _router: Router, private userService: UserService) { }

  ngOnInit(): void {
  }

  Registration():void{
    this.userService.registration(this.login, this.password).then(() => {
      this._router.navigate(['home']);
    },
    (Error: HttpErrorResponse)=>
      window.alert(Error.error.error)
    );
    
  }

  onChange():void{
    this.pattern=this.password;
  }
}
