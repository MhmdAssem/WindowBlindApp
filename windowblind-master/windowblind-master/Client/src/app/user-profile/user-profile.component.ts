import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../api.service';
import { AuthService } from '../auth.service';
import { GridColumn } from '../models/gridColumn';
import { User } from '../models/user';
import { UserSettings } from '../models/userSettings';

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss']
})
export class UserProfileComponent implements OnInit {

  constructor(private authService: AuthService, private snackBar: MatSnackBar) { }

  get currentUser(): User {
    if (!this.authService.currentUser){
      throw new Error('No current user');
    }
    if (!this.authService.currentUser.settings){
      this.authService.currentUser.settings = new UserSettings();
      this.authService.currentUser.settings.fabricCutterColumns = this.fabricCutterColumns;
    }
    return this.authService.currentUser;
   }


  get fabricCutterColumns(): GridColumn[] {return GridColumn.fabricCutterColumns; }

  compareColumns = (c1: GridColumn, c2: GridColumn) => c1.name === c2.name;

  ngOnInit(): void {
  }

  onSave(): void {
    this.authService.updateUser()
    .subscribe({
      complete: () => {this.snackBar.open('Settings updated', '', {duration: 3000}); }
    });
  }
  onCancel(): void {
    this.authService.resetUser();
  }
}
