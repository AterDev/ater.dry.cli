import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
} from '@angular/common/http';

import { catchError } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable, throwError } from 'rxjs';


@Injectable()
export class CustomHttpInterceptor implements HttpInterceptor {
  constructor(
    private snb: MatSnackBar
  ) { }
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          return this.handleError(error);
        })
      );
  }
  handleError(error: HttpErrorResponse) {
    let errors = {
      detail: '',
      status: 500,
    };
    if (error.status == 401) {
      errors.detail = '401:未授权的请求！';
    }
    if (error.status == 403) {
      errors.detail = '403:拒绝访问！';
    }
    if (error.status == 409) {
      errors.detail = '409:重复的资源！';
    }
    if (error.status == 500) {
      errors.detail = '500:内部错误';
    }
    if (!error.error) {
      if (error.message) {
        errors.detail = error.message;
      }
      errors.status = error.status;
    } else {
      errors = error.error;
    }
    return throwError(errors);
  }
}
