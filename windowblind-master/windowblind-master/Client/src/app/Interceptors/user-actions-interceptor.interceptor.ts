import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpResponse
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { FabricCutterCBDetailsModel, ResultModel } from '../fabric-cutter/FabricCutterCBDetailsModel';


@Injectable()
export class UserActionsInterceptorInterceptor implements HttpInterceptor {

  constructor() { }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<any> {

    return next.handle(request).pipe(
      map((event: HttpEvent<any>) => {
        if (event instanceof HttpResponse) {
          let model = event.body as unknown as ResultModel;
          if (model.status != undefined && model.status != 200) {
            alert("Error Message: " + event.body.message);
            console.log("StackTrace: " + event.body.stackTrace)
          }
        }
        return event;
      }));

  }

  public static ParseToDataModel(event: any) {
    let model = (event as unknown as ResultModel);
    return model.data;
  }
}

