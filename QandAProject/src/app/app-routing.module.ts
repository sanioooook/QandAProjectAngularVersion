import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegistrationComponent } from './registration/registration.component';
import { HomeComponent } from './home/home.component';
import { NotAuthorized } from './classes/not-authorized';
import { Authorized } from './classes/authorized';
import { AllSurveyComponent } from './all-survey/all-survey.component';
import { UserSurveysComponent } from './user-surveys/user-surveys.component';
import { SurveyComponent } from './survey/survey.component';
import { EditSurveyComponent } from './edit-survey/edit-survey.component';
import { CreateSurveyComponent } from './create-survey/create-survey.component';
import { UserVoteSurveysComponent } from './user-vote-surveys/user-vote-surveys.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent, canActivate: [NotAuthorized] },
  { path: 'registration', component: RegistrationComponent, canActivate: [NotAuthorized] },
  { path: 'home', component: HomeComponent },
  { path: 'all-surveys', component: AllSurveyComponent, canActivate: [Authorized] },
  { path: 'your-surveys', component: UserSurveysComponent, canActivate: [Authorized] },
  { path: 'survey/:id', component: SurveyComponent, canActivate: [Authorized] },
  { path: 'edit-survey/:id', component: EditSurveyComponent, canActivate: [Authorized] },
  { path: 'create-survey', component: CreateSurveyComponent, canActivate: [Authorized] },
  { path: 'user-vote-surveys', component: UserVoteSurveysComponent, canActivate: [Authorized] },
  { path: '**', redirectTo: 'home' },
  { path: '', redirectTo: 'home', pathMatch: 'full' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
