import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PreEzStopService {

  constructor(private httpClient: HttpClient) { }
  
  
  public getCBNumberDetails(LineNumber: string): Observable<boolean> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json', "LineNumber": LineNumber  });

    return this.httpClient
      .get<boolean>(environment.apiUrl + 'PreEzStop/GenerateXMLFile', { headers }).pipe(
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
