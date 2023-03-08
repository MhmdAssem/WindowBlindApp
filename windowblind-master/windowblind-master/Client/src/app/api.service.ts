import { Injectable, Type } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {environment} from '../environments/environment';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  constructor(private http: HttpClient) { }

  getAll<T>(url: string, searchString?: string): Observable<T[]> {
    const options = searchString ?
      { params: new HttpParams().set('searchString', searchString) } : {};
    return this.http.get<T[]>(environment.apiUrl + url, options);
  }

  getType<T>(url: string, type: any): Observable<T[]> {
    return this.getAll<T>(url).pipe(
      tap(x => x.forEach(xx => Object.setPrototypeOf(xx, type.prototype))));
  }

  call(url: string, data: any): Observable<void>{
    return this.http.post<any>(environment.apiUrl + url, data);
  }

  // get(id): Observable<any> {
  //   return this.http.get(`${baseUrl}/${id}`);
  // }

  create(url: string, data: any): Observable<any> {
    return this.http.post<any>(environment.apiUrl + url, data);
  }
  delete(url: string, id: any): Observable<any> {
    return this.http.delete<any>(`${environment.apiUrl}${url}/${id}`);
  }
  update(url: string, id: any, data: any): Observable<any> {
    return this.http.put<any>(`${environment.apiUrl}${url}/${id}`, data);
  }


}
