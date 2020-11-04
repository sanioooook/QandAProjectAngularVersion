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
import { MatButtonModule } from '@angular/material/button';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  NgxMatDatetimePickerModule,
  NgxMatNativeDateModule,
  NgxMatTimepickerModule
} from '@angular-material-components/datetime-picker';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatCardModule } from '@angular/material/card';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSortModule } from '@angular/material/sort';
import { MatPaginatorIntl, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { OverlayModule } from '@angular/cdk/overlay';
import { MatMenuModule } from '@angular/material/menu';
import { TranslocoRootModule } from './transloco/transloco-root.module';
import { MatPaginatorIntlCro } from './services/mat-paginator-intl-cro.service';

import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { RegistrationComponent } from './registration/registration.component';
import { HomeComponent } from './home/home.component';
import { AllSurveyComponent } from './all-survey/all-survey.component';
import { SurveyComponent } from './survey/survey.component';
import { EditSurveyComponent } from './edit-survey/edit-survey.component';
import { CreateSurveyComponent, SettingDialog } from './create-survey/create-survey.component';
import { LoginOrRegistrationComponent } from './login-or-registration/login-or-registration.component';

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
    SettingDialog,
    LoginOrRegistrationComponent,
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
    MatPaginatorModule,
    MatButtonModule,
    MatToolbarModule,
    MatTooltipModule,
    NgxMatDatetimePickerModule,
    NgxMatTimepickerModule,
    NgxMatNativeDateModule,
    MatDatepickerModule,
    MatCardModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTabsModule,
    OverlayModule,
    MatMenuModule,
    TranslocoRootModule
  ],
  providers: [
    { provide: MatPaginatorIntl, useClass: MatPaginatorIntlCro },
    { provide: ErrorStateMatcher, useClass: ShowOnDirtyErrorStateMatcher }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
