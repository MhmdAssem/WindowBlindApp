import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { CreateFileAndLabelModel } from '../fabric-cutter/CreateFileAndLabelModel';
import { FabricCutterCBDetailsModel } from '../fabric-cutter/FabricCutterCBDetailsModel';

@Injectable({
  providedIn: 'root'
})
export class LogCutService {

  constructor(private httpClient: HttpClient) { }

  
  public ClearOrdersFromLogCut(Data: FabricCutterCBDetailsModel, UserName, tableName): Observable<FabricCutterCBDetailsModel> {

    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    let model: CreateFileAndLabelModel = {
      data: Data,
      tableName: tableName,
      userName: UserName,
      printer: "None"
    }

    return this.httpClient
      .post<any>(environment.apiUrl + 'LogCut/ClearOrdersFromLogCut', model, { headers }).pipe(
        tap(
          data => {
          }
        )

      );
  }
  
  public getCBNumberDetails(CBNumber: string): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', "CBNumberOrLineNumber": CBNumber });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'LogCut/getCBNumberDetails', { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Sorry Configuration is not done , please contact your admin !");
            }
          }
        )

      );
  }


  public LogCutSend(TableNumber: string, printer: string, UserName: string, Data: FabricCutterCBDetailsModel): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    let model: CreateFileAndLabelModel = {
      data: Data,
      tableName: TableNumber,
      userName: UserName,
      printer: printer
    }
    return this.httpClient
      .post<any>(environment.apiUrl + 'LogCut/LogCutSend', model, { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Sorry Configuration is not done , please contact your admin !");
            }
          }
        )

      );
  }

  
  public GetHeldObjects(tableName): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json',"tableName":tableName  });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'LogCut/GetHeldObjects', { headers }).pipe(
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
