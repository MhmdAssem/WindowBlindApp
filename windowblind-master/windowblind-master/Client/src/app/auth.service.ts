import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of, throwError } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { ApiService } from './api.service';
import { Station } from './models/station';
import { Table } from './models/table';
import { User } from './models/user';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  currentUser?: User = undefined;
  currentTable?: Table = undefined;
  currentStation?: Station = undefined;

  login(user: User): Observable<User> {
    return this.http.post<User>(environment.apiUrl + "Users/Login", user).pipe(
      map(uu => uu),
      tap(val => {
        this.currentUser = val;
        localStorage.setItem('currentUser', JSON.stringify(this.currentUser));
        localStorage.setItem('UserName',  this.currentUser.name);
        console.log('User Authentication is successful: ' + val);
      })
    );
  }

  setTable(position: [Table, Station]): void {
    this.currentTable = position[0];
    this.currentStation = position[1];
    localStorage.setItem('currentTable', JSON.stringify(this.currentTable));
    localStorage.setItem('currentStation', JSON.stringify(this.currentStation));
  }

  logout(): void {
    this.currentUser = undefined;
    localStorage.removeItem('currentUser');
  }

  constructor(private apiService: ApiService, private http: HttpClient) {
    this.resetUser();
    const storageTable = localStorage.getItem('currentTable');
    if (storageTable) {
      this.currentTable = JSON.parse(storageTable);
    }
    const storageStation = localStorage.getItem('currentStation');
    if (storageStation) {
      this.currentStation = JSON.parse(storageStation);
    }
  }

  updateUser(): Observable<void> {
    if (!this.currentUser) { return throwError('Not signed in'); }
    return this.apiService.update('users', this.currentUser.id, this.currentUser)
      .pipe(tap({ complete: () => { localStorage.setItem('currentUser', JSON.stringify(this.currentUser)); } }));
  }

  resetUser(): void {
    const storageUser = localStorage.getItem('currentUser');
    if (storageUser) {
      try { this.currentUser = JSON.parse(storageUser); }
      catch { localStorage.removeItem('currentUser'); }

    }

  }

}
