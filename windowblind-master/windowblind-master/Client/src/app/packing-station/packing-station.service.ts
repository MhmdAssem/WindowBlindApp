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
export class PackingStationService {
  
  ClearOrdersFromPacking(Model: FabricCutterCBDetailsModel, UserName: any, tableName: string) {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    let model: CreateFileAndLabelModel = {
      data: Model,
      tableName: tableName,
      userName: UserName,
      printer: "None"
    }

    return this.httpClient
      .post<any>(environment.apiUrl + 'PackingStation/ClearOrdersFromPacking', model, { headers }).pipe(
        tap(
          data => {
          }
        )

      );
  }
  constructor(private httpClient: HttpClient) { }

  public GetReadyToPack(input): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json',"CbOrLineNumber":input });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'PackingStation/GetReadyToPack', { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Sorry Configuration is not done , please contact your admin !");
            }
          }
        )

      );
  }

  public pushLinesNoToHoistStation(TableNumber: string, printer: string, UserName: string, Data: FabricCutterCBDetailsModel): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    let model: CreateFileAndLabelModel = {
      data: Data,
      tableName: TableNumber,
      userName: UserName,
      printer: printer
    }
    return this.httpClient
      .post<any>(environment.apiUrl + 'PackingStation/pushLinesNoToPackingStation', model, { headers }).pipe(
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
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', 'tableName':tableName });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'PackingStation/GetHeldObjects', { headers }).pipe(
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
