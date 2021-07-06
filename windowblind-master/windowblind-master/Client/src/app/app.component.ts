import { Component } from '@angular/core';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  constructor(private authService: AuthService)
  {}

  get isAdministrator(): boolean {
    return this.authService.currentUser?.role == "Admin"; }
  get currentTable(): string | undefined { return this.authService.currentTable?.name; }
  get currentStation(): string | undefined { return this.authService.currentStation?.name; }
}
