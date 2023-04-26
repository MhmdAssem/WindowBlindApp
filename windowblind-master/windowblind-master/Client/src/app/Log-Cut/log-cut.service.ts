import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { CreateFileAndLabelModel } from '../fabric-cutter/CreateFileAndLabelModel';
import { FabricCutterCBDetailsModel } from '../fabric-cutter/FabricCutterCBDetailsModel';
import { UserActionsInterceptorInterceptor } from '../Interceptors/user-actions-interceptor.interceptor';

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
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )
      );
  }
  
  public getCBNumberDetails(CBNumber: string): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', "CBNumberOrLineNumber": CBNumber });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'LogCut/getCBNumberDetails', { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
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
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )
      );
  }

  
  public GetHeldObjects(tableName): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json',"tableName":tableName  });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'LogCut/GetHeldObjects', { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )

      );
  }
  
  
  public GetDataUsingAutoUpload(TableName: string, UserName: string, ShiftTable: string, Type: string): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', "TableName": TableName, "UserName": UserName, "Shift": ShiftTable, "Type": Type });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'LogCut/GetDataUsingAutoUpload', { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )

      );
  }


  public UpdateRows(ids: string[]): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', });

    return this.httpClient
      .post<FabricCutterCBDetailsModel>(environment.apiUrl + 'LogCut/UpdateRows', ids, { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )

      );
  }

}
