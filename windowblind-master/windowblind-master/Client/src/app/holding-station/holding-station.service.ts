import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { CreateFileAndLabelModel } from '../fabric-cutter/CreateFileAndLabelModel';
import { FabricCutterCBDetailsModel } from '../fabric-cutter/FabricCutterCBDetailsModel';
import { User } from '../models/user';
import { OrdersApprovalModel } from './OrdersApprovalModel';
import { ReasonModel } from './ReasonModel';
import { RejectionModel } from './RejectionModel';

@Injectable({
  providedIn: 'root'
})
export class HoldingStationService {

  ClearOrdersFromHoldingStation(Model: RejectionModel[], UserName: any): Observable<FabricCutterCBDetailsModel> {

    const headers = new HttpHeaders({ 'Content-Type': 'application/json', 'UserName': UserName });

    return this.httpClient
      .post<any>(environment.apiUrl + 'HoldingStation/ClearOrdersFromHoldingStation', Model, { headers }).pipe(
        tap(
          data => {
          }
        )

      );
  }
  UpdateReasonsForHeldObject(model: ReasonModel): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .post<boolean>(environment.apiUrl + 'HoldingStation/UpdateReasonsForHeldObject', model, { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Error Happend while updating this row!");
            }
          }
        )

      );


  }

  constructor(private httpClient: HttpClient) { }

  public RejectThisRow(model: RejectionModel[]): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .post<boolean>(environment.apiUrl + 'HoldingStation/RejectThisRow', model, { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Error Happend while holding this row!");
            }
          }
        )

      );
  }

  public ApproveThisOrders(model: OrdersApprovalModel): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .post<boolean>(environment.apiUrl + 'HoldingStation/ApproveThisOrders', model, { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Error Happend while Sending these data!");
            }
          }
        )

      );
  }

  public GetAllRejectedOrders(): Observable<RejectionModel[]> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .get<RejectionModel[]>(environment.apiUrl + 'HoldingStation/GetAllRejectedOrders', { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Error Happend while holding this row!");
            }
          }
        )

      );
  }


  public SaveAdminNotes(model: RejectionModel): Observable<any> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .post<any>(environment.apiUrl + 'HoldingStation/SaveAdminNotes', model, { headers }).pipe(
        tap(
          data => {
            if (!data) {
              alert("Error Happend while holding this row!");
            }
          }
        )

      );
  }

}
