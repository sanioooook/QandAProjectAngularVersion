import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllSurveyComponent } from './all-survey.component';

describe('AllSurveyComponent', () => {
  let component: AllSurveyComponent;
  let fixture: ComponentFixture<AllSurveyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllSurveyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllSurveyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
