import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { LoginOrRegistrationComponent } from './login-or-registration.component';

describe('LoginOrRegistrationComponent', () => {
  let component: LoginOrRegistrationComponent;
  let fixture: ComponentFixture<LoginOrRegistrationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ LoginOrRegistrationComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LoginOrRegistrationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
