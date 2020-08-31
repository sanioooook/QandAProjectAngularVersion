import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { HttpClientModule } from '@angular/common/http';
import { CovalentLayoutModule } from '@covalent/core/layout';
import { CovalentStepsModule } from '@covalent/core/steps';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { CovalentDialogsModule } from '@covalent/core/dialogs';
import { CovalentDynamicFormsModule } from '@covalent/dynamic-forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { ErrorStateMatcher, ShowOnDirtyErrorStateMatcher } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { CovalentLoadingModule } from '@covalent/core/loading';
import { MatIconModule } from '@angular/material/icon';

import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { RegistrationComponent } from './registration/registration.component';
import { HomeComponent } from './home/home.component';
import { AllSurveyComponent } from './all-survey/all-survey.component';
import { SurveyComponent } from './survey/survey.component';
import { EditSurveyComponent } from './edit-survey/edit-survey.component';
import { CreateSurveyComponent } from './create-survey/create-survey.component';
import { MatSortModule } from '@angular/material/sort';
import { MatPaginatorModule } from '@angular/material/paginator';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegistrationComponent,
    HomeComponent,
    AllSurveyComponent,
    SurveyComponent,
    EditSurveyComponent,
    CreateSurveyComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    HttpClientModule,
    CovalentLayoutModule,
    CovalentStepsModule,
    BrowserAnimationsModule,
    CovalentDialogsModule,
    CovalentDynamicFormsModule,
    MatCheckboxModule,
    MatInputModule,
    ReactiveFormsModule,
    MatTableModule,
    CovalentLoadingModule,
    MatIconModule,
    MatSortModule,
    MatPaginatorModule
  ],
  providers: [
    {provide: ErrorStateMatcher, useClass: ShowOnDirtyErrorStateMatcher}
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
