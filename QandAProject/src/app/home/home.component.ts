import { Component, OnInit } from '@angular/core';
import { UserService } from '../services/user-service.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  constructor(public userService: UserService) { }

  private linksNotAuthorizedUser = [
    { title: 'Login', link: '/login' },
    { title: 'Registration', link: '/registration' }
  ];
  private linksAuthorizedUser = [
    { title: 'Create survey', link: '/create-survey' },
    { title: 'All surveys', link: '/all-surveys' }
  ];

  ngOnInit(): void {
  }

  getLinks(): any {
    if (this.userService.IsUserLogged()) {
      return this.linksAuthorizedUser;
    }
    else {
      return this.linksNotAuthorizedUser;
    }
  }

}
