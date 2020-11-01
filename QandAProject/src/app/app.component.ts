import { Component, HostBinding, OnInit } from '@angular/core';
import { UserService } from './services/user-service.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { OverlayContainer} from '@angular/cdk/overlay';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  constructor(public userService: UserService,
              public overlayContainer: OverlayContainer,
              private router: Router) {
  }
  title = 'QandA';
  previusTheme = '';
  @HostBinding('class') componentCssClass: string;

  ngOnInit(): void {
    let theme = localStorage.getItem('theme');
    if (!theme) {
      theme = 'dark-theme';
    }
    this.onSetTheme(theme);
  }

  logOut(): void {
    this.userService.logOut().then(
      _ => this.router.navigate(['/login-or-registration']),
      (err: HttpErrorResponse) =>
        console.log(err)
    );
  }

  onSetTheme(theme: string): void {
    if (theme !== this.componentCssClass) {
      if (!!this.componentCssClass) {
        this.overlayContainer.getContainerElement().classList.remove(this.componentCssClass);
      }
      this.overlayContainer.getContainerElement().classList.add(theme);
      this.componentCssClass = theme;
      localStorage.setItem('theme', theme);
    }
  }
}
