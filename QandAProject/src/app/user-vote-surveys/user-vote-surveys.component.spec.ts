import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { UserVoteSurveysComponent } from './user-vote-surveys.component';

describe('UserVoteSurveysComponent', () => {
  let component: UserVoteSurveysComponent;
  let fixture: ComponentFixture<UserVoteSurveysComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ UserVoteSurveysComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(UserVoteSurveysComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
