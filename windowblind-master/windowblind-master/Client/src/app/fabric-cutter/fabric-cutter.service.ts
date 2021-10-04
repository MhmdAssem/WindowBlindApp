import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { type } from 'os';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { CreateFileAndLabelModel } from './CreateFileAndLabelModel';
import { FabricCutterCBDetailsModel } from './FabricCutterCBDetailsModel';

@Injectable({
  providedIn: 'root'
})
export class FabricCutterService {

  constructor(private httpClient: HttpClient) { }


  public getCBNumberDetails(CBNumber: string): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', "CBNumber": CBNumber });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'FabricCutter/getCBNumberDetails', { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Sorry Configuration is not done , please contact your admin !");
            }
          }
        )

      );
  }


  public CreateFilesAndLabels(TableNumber: string, printer: string, UserName: string, Data: FabricCutterCBDetailsModel): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    let model: CreateFileAndLabelModel = {
      data: Data,
      tableName: TableNumber,
      userName: UserName,
      printer: printer
    }
    return this.httpClient
      .post<any>(environment.apiUrl + 'FabricCutter/CreateFilesAndLabels', model, { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Sorry Configuration is not done , please contact your admin !");
            }
          }
        )

      );
  }

  public PrintLabels(TableNumber: string, printer: string, UserName: string, Data: FabricCutterCBDetailsModel): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    let model: CreateFileAndLabelModel = {
      data: Data,
      tableName: TableNumber,
      userName: UserName,
      printer: printer
    }
    return this.httpClient
      .post<any>(environment.apiUrl + 'FabricCutter/PrintLabelsOnly', model, { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Sorry Configuration is not done , please contact your admin !");
            }
          }
        )

      );
  }

  public GetDataUsingAutoUpload(TableName: string,UserName:string,ShiftTable:string,Type:string): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', "TableName": TableName,"UserName":UserName,"Shift":ShiftTable,"Type":Type });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'FabricCutter/GetDataUsingAutoUpload', { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Sorry Configuration is not done , please contact your admin !");
            }
          }
        )

      );
  }

  
  public UpdateRows(ids:string[] ): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json',});

    return this.httpClient
      .post<FabricCutterCBDetailsModel>(environment.apiUrl + 'FabricCutter/UpdateRows',ids, { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Sorry Configuration is not done , please contact your admin !");
            }
          }
        )

      );
  }

  
  public GetHeldObjects(tableName:string): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', "tableName":tableName });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'FabricCutter/GetHeldObjects', { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Sorry Configuration is not done , please contact your admin !");
            }
          }
        )

      );
  }

  
}
