import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { FileSettings } from '../models/FileSettings';

@Injectable({
  providedIn: 'root'
})
export class SettingService {


  constructor(private httpClient: HttpClient) { }

  public getSettings(): Observable<FileSettings[]> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .get<FileSettings[]>(environment.apiUrl + 'Settings/GetAllSettings', { headers });
  }

  public getColumnsNames(): Observable<[]> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .get<[]>(environment.apiUrl + 'Settings/getColumnsNames', { headers });
  }

  public GetPrinterNames(): Observable<[]> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .get<[]>(environment.apiUrl + 'Settings/GetPrinterNames', { headers });
  }


  public UpdateSettings(list: FileSettings[]): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    console.log(list);
    return this.httpClient
      .post<any>(environment.apiUrl + 'Settings/UpdateAllSettings', list, { headers });
  }

  public getTableNumber(name: string): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', 'name': name });
    return this.httpClient
      .get<any>(environment.apiUrl + 'Settings/getTableNumber', { headers });
  }

}
