import { Component, OnInit } from '@angular/core';
import { UserService } from '../services/user-service.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {

  constructor(public userService:UserService) { }

  linksNotAuthorizedUser = [
    {title: 'Login', link: '/login'},
    {title: 'Registration', link: '/registration'}
  ];
  linksAuthorizedUser = [
    {title: 'All surveys', link: '/all-surveys'},
    {title: 'Your surveys', link: '/your-surveys'},
    {title: 'Polls in which you voted', link: '/your-voted-surveys'}
  ];

  ngOnInit(): void {
  }

}
