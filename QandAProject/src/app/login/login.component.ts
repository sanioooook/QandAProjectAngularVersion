import { Component } from '@angular/core';
import { UserService } from '../services/user-service.service';
import { Router } from '@angular/router';

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
    .then(() => this.router.navigate(['home']));
  }
}
