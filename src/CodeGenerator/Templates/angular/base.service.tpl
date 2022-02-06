import { Injectable, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
export const BLOB_BASE_URL = new InjectionToken<string>('BLOB_BASE_URL');
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

@Injectable({
  providedIn: 'root'
})
export class BaseService {
  baseUrl: string | null;
  isMobile = false;
  constructor(
    public http: HttpClient,
  ) {
    this.isMobile = this.isMoblie();
    this.baseUrl = environment.api_daemon;
  }

  request<R>(method: string, path: string, body?: any): Observable<R> {
    const url = this.baseUrl + path;
    const options = {
      headers: new HttpHeaders({
        Accept: 'application/json',
        Authorization: 'Bearer',
      }),
      body
    };
    if (localStorage.getItem('token')) {
      options.headers = options.headers.set('Authorization', 'Bearer ' + localStorage.getItem('token'));
      options.headers = options.headers.set('Client', 'webapp');
    }
    return this.http.request<R>(method, url, options);
  }


  downloadFile(method: string, path: string, body?: any): Observable<Blob> {
    const url = this.baseUrl + path;
    const options = {
      responseType: 'blob' as 'blob',
      headers: new HttpHeaders({
        Authorization: 'Bearer ' + localStorage.getItem('token'),
        Client: 'webapp'
      }),
      body,
    };
    if (localStorage.getItem('token')) {
      options.headers = options.headers.set('Authorization', 'Bearer ' + localStorage.getItem('token'));
      options.headers = options.headers.set('Client', 'webapp');
    }
    return this.http.request(method, url, options);
  }


  isMoblie(): boolean {
    const ua = navigator.userAgent;
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Mobile|mobile|CriOS/i.test(ua)) {
      return true;
    }
    return false;
  }
}
export interface ErrorResult {
  title: string;
  detail: string;
  status: number;
}
