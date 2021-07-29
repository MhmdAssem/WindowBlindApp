import { Component } from '@angular/core';
import { AuthService } from './auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  constructor(private authService: AuthService) { }

  get isAdministrator(): boolean {
    return this.authService.currentUser?.role == "Admin";
  }

  get isCustomerService(): boolean {
    return this.authService.currentUser?.role == "Customer service";
  }
  get isFactoryStaff(): boolean {
    return this.authService.currentUser?.role == "Factory staff";
  }
  get currentTable(): string | undefined { return this.authService.currentTable?.name; }
  get currentStation(): string | undefined { return this.authService.currentStation?.name; }
}
