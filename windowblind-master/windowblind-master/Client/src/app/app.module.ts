import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { FabricCutterComponent } from './fabric-cutter/fabric-cutter.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LogoutComponent } from './logout/logout.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { OverlayModule } from '@angular/cdk/overlay';
import { CdkTreeModule } from '@angular/cdk/tree';
import { PortalModule } from '@angular/cdk/portal';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatChipsModule } from '@angular/material/chips';
import { MatRippleModule } from '@angular/material/core';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTreeModule } from '@angular/material/tree';
import { MatBadgeModule } from '@angular/material/badge';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatRadioModule } from '@angular/material/radio';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FlexLayoutModule } from '@angular/flex-layout';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { LabelsComponent } from './labels/labels.component';

import { NgxBarcode6Module } from 'ngx-barcode6';
import { UsersComponent } from './users/users.component';
import { LayoutModule } from '@angular/cdk/layout';
import { UserDialogComponent } from './user-dialog/user-dialog.component';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog.component';
import { MatDialogModule } from '@angular/material/dialog';
import { TableDialogComponent } from './table-dialog/table-dialog.component';
import { SettingsComponent } from './settings/settings.component';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { HomeComponent } from './Home/home.component';
import { DataTableDirective,DataTablesModule  } from 'angular-datatables';
import { LogCutComponent } from './Log-Cut/log-cut.component';
import { RollWidthDialogComponent } from './fabric-cutter/roll-width-dialog/roll-width-dialog.component';
import { EzStopComponent } from './ez-stop/ez-stop.component';
import { AssemblyStationComponent } from './assembly-station/assembly-station.component';
import { HoistStationComponent } from './hoist-station/hoist-station.component';
import { PackingStationComponent } from './packing-station/packing-station.component';
import { ReportStationComponent } from './report-station/report-station.component';
import { HoldingStationComponent } from './holding-station/holding-station.component';
import { PreEzStopComponent } from './Pre-EzStop/pre-ez-stop.component';
import { TablesComponent } from './tables/tables.component';
import { AdminNotesModelComponent } from './report-station/Admin_Notes_Model/admin-notes-model/admin-notes-model.component';
import { UserActionsInterceptorInterceptor } from './Interceptors/user-actions-interceptor.interceptor';


const materialModules = [
  CdkTreeModule,
  MatAutocompleteModule,
  MatButtonModule,
  MatCardModule,
  MatCheckboxModule,
  MatChipsModule,
  MatDividerModule,
  MatExpansionModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatMenuModule,
  MatProgressSpinnerModule,
  MatPaginatorModule,
  MatRippleModule,
  MatSelectModule,
  MatSidenavModule,
  MatSnackBarModule,
  MatSortModule,
  MatTableModule,
  MatTabsModule,
  MatToolbarModule,
  MatFormFieldModule,
  MatButtonToggleModule,
  MatTreeModule,
  OverlayModule,
  PortalModule,
  MatBadgeModule,
  MatGridListModule,
  MatRadioModule,
  MatDatepickerModule,
  MatTooltipModule,
  MatSlideToggleModule,
  MatDialogModule,

];
@NgModule({
  entryComponents:[
    RollWidthDialogComponent
  ],
  declarations: [
    AppComponent,
    LoginComponent,
    FabricCutterComponent,
    LogoutComponent,
    LabelsComponent,
    UsersComponent,
    UserDialogComponent,
    ConfirmDialogComponent,
    TableDialogComponent,
    SettingsComponent,
    UserProfileComponent,
    HomeComponent,
    LogCutComponent,
    RollWidthDialogComponent,
    EzStopComponent,
    AssemblyStationComponent,
    HoistStationComponent,
    PackingStationComponent,
    ReportStationComponent,
    HoldingStationComponent,
    PreEzStopComponent,
    TablesComponent,
    AdminNotesModelComponent,
  ],
  imports: [
    ...materialModules,
    CommonModule,
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    FlexLayoutModule,
    HttpClientModule,
    NgxBarcode6Module,
    MatGridListModule,
    MatCardModule,
    MatMenuModule,
    MatIconModule,
    MatButtonModule,
    LayoutModule,
    NgbModule,
    FontAwesomeModule,
    DataTablesModule
  ],
  exports: [
    ...materialModules,
  ],
  providers: [{provide: HTTP_INTERCEPTORS, useClass: UserActionsInterceptorInterceptor, multi: true}],
  bootstrap: [AppComponent]
})
export class AppModule { }
