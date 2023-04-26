import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { CreateFileAndLabelModel } from '../fabric-cutter/CreateFileAndLabelModel';
import { FabricCutterCBDetailsModel } from '../fabric-cutter/FabricCutterCBDetailsModel';
import { User } from '../models/user';
import { OrdersApprovalModel } from './OrdersApprovalModel';
import { ReasonModel } from './ReasonModel';
import { RejectionModel } from './RejectionModel';
import { UserActionsInterceptorInterceptor } from '../Interceptors/user-actions-interceptor.interceptor';

@Injectable({
  providedIn: 'root'
})
export class HoldingStationService {

  ClearOrdersFromHoldingStation(Model: RejectionModel[], UserName: any): Observable<FabricCutterCBDetailsModel> {

    const headers = new HttpHeaders({ 'Content-Type': 'application/json', 'UserName': UserName });

    return this.httpClient
      .post<any>(environment.apiUrl + 'HoldingStation/ClearOrdersFromHoldingStation', Model, { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )

      );
  }
  UpdateReasonsForHeldObject(model: ReasonModel): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .post<boolean>(environment.apiUrl + 'HoldingStation/UpdateReasonsForHeldObject', model, { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )
      );
  }

  constructor(private httpClient: HttpClient) { }

  public RejectThisRow(model: RejectionModel[]): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .post<boolean>(environment.apiUrl + 'HoldingStation/RejectThisRow', model, { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )
      );
  }

  public ApproveThisOrders(model: OrdersApprovalModel): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .post<boolean>(environment.apiUrl + 'HoldingStation/ApproveThisOrders', model, { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )
      );
  }

  public GetAllRejectedOrders(): Observable<RejectionModel[]> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .get<RejectionModel[]>(environment.apiUrl + 'HoldingStation/GetAllRejectedOrders', { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )

      );
  }

  public SaveAdminNotes(model: RejectionModel): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .post<any>(environment.apiUrl + 'HoldingStation/SaveAdminNotes', model, { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )
      );
  }
}
