import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
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
  public GetTablesBasedOnApplication(application: string): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', 'application': application });
    return this.httpClient
      .get<any>(environment.apiUrl + 'Settings/GetTablesBasedOnApplication', { headers });
  }


  public GetSearchType(applicationType): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json','applicationType': applicationType});

    return this.httpClient
      .get<any>(environment.apiUrl + 'Settings/GetSearchType', { headers }).pipe(
        tap(
          data => {
            if (data == null) {
              alert("Sorry Configuration is not done , please contact your admin !");
            }
          }
        )

      );
  }

  public CheckTableExists(name: string) {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json',"name": name });

    return this.httpClient
      .get<any>(environment.apiUrl + 'Settings/CheckTableExists',   { headers }).pipe();
  }
  
  

  public InsertTableNameWithThePath(TableName: string, OutputPath: string) {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json',"TableName": TableName, "OutputPath": OutputPath });

    return this.httpClient
      .get<any>(environment.apiUrl + 'Settings/InsertTableNameWithThePath',{ headers }).pipe();
  }

  
  public DeleteTable(name: string) {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json',"name": name });

    return this.httpClient
      .get<any>(environment.apiUrl + 'Settings/DeleteTable',   { headers }).pipe();
  }
  
}
