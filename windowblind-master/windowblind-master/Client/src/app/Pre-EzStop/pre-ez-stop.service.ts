import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { UserActionsInterceptorInterceptor } from '../Interceptors/user-actions-interceptor.interceptor';

@Injectable({
  providedIn: 'root'
})
export class PreEzStopService {

  constructor(private httpClient: HttpClient) { }
  
  
  public getCBNumberDetails(LineNumber: string): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', "LineNumber": LineNumber  });

    return this.httpClient
      .get<boolean>(environment.apiUrl + 'PreEzStop/GenerateXMLFile', { headers }).pipe(
        map(
          data => {
            return UserActionsInterceptorInterceptor.ParseToDataModel(data);
          }
        )

      );
  }

}
