import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { AssemblyStationComponent } from './assembly-station/assembly-station.component';
import { AuthGuard } from './auth.guard';
import { EzStopComponent } from './ez-stop/ez-stop.component';
import { FabricCutterComponent } from './fabric-cutter/fabric-cutter.component';
import { HoistStationComponent } from './hoist-station/hoist-station.component';
import { HoldingStationComponent } from './holding-station/holding-station.component';
import { HomeComponent } from './Home/home.component';
import { LogCutComponent } from './Log-Cut/log-cut.component';
import { LoginComponent } from './login/login.component';
import { PackingStationComponent } from './packing-station/packing-station.component';
import { PreEzStopComponent } from './Pre-EzStop/pre-ez-stop.component';
import { ReportStationComponent } from './report-station/report-station.component';
import { SettingsComponent } from './settings/settings.component';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { UsersComponent } from './users/users.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'fabric-cutter', component: FabricCutterComponent, canActivate: [AuthGuard] },
  { path: 'log-cut', component: LogCutComponent, canActivate: [AuthGuard] },
  { path: 'Ez-Stop', component: EzStopComponent, canActivate: [AuthGuard] },
  { path: 'users', component: UsersComponent, canActivate: [AuthGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [AuthGuard] },
  { path: 'profile', component: UserProfileComponent, canActivate: [AuthGuard] },
  { path: 'Assembly-Station', component: AssemblyStationComponent, canActivate: [AuthGuard] },
  { path: 'Hoist-Station', component: HoistStationComponent, canActivate: [AuthGuard] },
  { path: 'Packing-Station', component: PackingStationComponent, canActivate: [AuthGuard] },
  { path: 'Report-Station', component: ReportStationComponent, canActivate: [AuthGuard] },
  { path: 'Holding-Station', component: HoldingStationComponent, canActivate: [AuthGuard] },
  { path: 'Pre-EzStop', component: PreEzStopComponent, canActivate: [AuthGuard] },
  { path: '', redirectTo: '/Home', pathMatch: 'full' },
  { path: 'Home', component: HomeComponent, canActivate:[AuthGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
