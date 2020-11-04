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
    { title: 'homeComponent.login', link: '/login' },
    { title: 'homeComponent.registration', link: '/registration' }
  ];
  private linksAuthorizedUser = [
    { title: 'homeComponent.createSurvey', link: '/create-survey' },
    { title: 'homeComponent.allSurveys', link: '/all-surveys' }
  ];

  ngOnInit(): void {
  }

  getLinks(): any {
    if (this.userService.IsUserLogged()) {
      return this.linksAuthorizedUser;
    }
    return this.linksNotAuthorizedUser;
  }

}
