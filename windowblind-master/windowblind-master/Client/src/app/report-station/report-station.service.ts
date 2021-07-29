import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { FabricCutterCBDetailsModel } from '../fabric-cutter/FabricCutterCBDetailsModel';

@Injectable({
  providedIn: 'root'
})
export class ReportStationService {
  constructor(private httpClient: HttpClient) { }

  public GenerateReports(): Observable<FabricCutterCBDetailsModel> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.httpClient
      .get<FabricCutterCBDetailsModel>(environment.apiUrl + 'ReportStation/GenerateReports', { headers }).pipe(
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
