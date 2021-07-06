import { Injectable, Type } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  constructor(private http: HttpClient) { }

  getAll(): Observable<User[]> {
    return this.http.get<User[]>(environment.apiUrl + "Users/GetAll");
  }
  delete(id: any): Observable<any> {
    return this.http.get<any>(environment.apiUrl + "Users/DeleteUser?userId="+id);
  }
  create(data: User): Observable<User> {
    return this.http.post<User>(environment.apiUrl + "Users", data);
  }
  update(data: User): Observable<User> {
    return this.http.put<User>(environment.apiUrl + "Users", data);
  }


}
