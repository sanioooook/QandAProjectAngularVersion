import { Component, HostBinding, OnInit } from '@angular/core';
import { UserService } from './services/user-service.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { OverlayContainer} from '@angular/cdk/overlay';
import { TranslocoService } from '@ngneat/transloco';
import * as moment from 'moment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  constructor(public userService: UserService,
              public overlayContainer: OverlayContainer,
              private translocoService: TranslocoService,
              private router: Router) {
  }
  title = 'QandA';
  @HostBinding('class') componentCssClass: string;

  ngOnInit(): void {
    let theme = localStorage.getItem('theme');
    let storageLang = localStorage.getItem('lang');
    if (!theme) {
      theme = 'dark-theme';
    }
    if (!storageLang) {
      storageLang = this.getLangs()[0];
    }
    this.onSetTheme(theme);
    this.onSetLang(storageLang);
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

  getLangs(): Array<any>{
    return ['en', 'ru'];
  }

  onSetLang(lang: string): void {
    this.translocoService.setActiveLang(lang);
    moment.locale(lang);
    localStorage.setItem('lang', lang);
  }
}
