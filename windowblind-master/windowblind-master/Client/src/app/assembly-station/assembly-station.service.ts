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
export class AssemblyStationService {
  
  
  ClearOrdersFromAssembly(Model: FabricCutterCBDetailsModel, UserName: any, tableName: string) {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    let model: CreateFileAndLabelModel = {
      data: Model,
      tableName: tableName,
      userName: UserName,
      printer: "None"
    }

    return this.httpClient
      .post<any>(environment.apiUrl + 'AssemblyStation/ClearOrdersFromAssembly', model, { headers }).pipe(
        map(
          data => {
          }
        )

      );
  }

  constructor(private httpClient: HttpClient) { }
  
  public GetReadyToAssemble(input:string): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json',"CbOrLineNumber":input });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'AssemblyStation/GetReadyToAssemble', { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )

      );
  }

  public pushLinesNoToAssemblyStation(TableNumber: string, printer: string, UserName: string, Data: FabricCutterCBDetailsModel): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    let model: CreateFileAndLabelModel = {
      data: Data,
      tableName: TableNumber,
      userName: UserName,
      printer: printer
    }
    return this.httpClient
      .post<any>(environment.apiUrl + 'AssemblyStation/pushLinesNoToAssemblyStation', model, { headers }).pipe(
        map(
          data => {
          }
        )

      );
  }
  
  
  
  public GetHeldObjects(tableName): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json',"tableName":tableName  });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'AssemblyStation/GetHeldObjects', { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )

      );
  }

  
}
