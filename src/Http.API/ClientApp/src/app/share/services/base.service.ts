import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
// import { OidcSecurityService } from 'angular-auth-oidc-client';

@Injectable({
  providedIn: 'root'
})
export class BaseService {
  private baseUrl: string | null;
  private isMobile = false;
  constructor(
    private http: HttpClient,
    // private oidcSecurityService: OidcSecurityService
  ) {
    this.isMobile = this.isMoblie();
    this.baseUrl = environment.api_daemon;
  }

  request<R>(method: string, path: string, body?: any): Observable<R> {
    const url = this.baseUrl + path;
    const options = {
      headers: this.getHeaders(),
      body
    };
    return this.http.request<R>(method, url, options);
  }

  downloadFile(method: string, path: string, body?: any): Observable<Blob> {
    const url = this.baseUrl + path;
    const options = {
      responseType: 'blob' as 'blob',
      headers: this.getHeaders(),
      body,
    };
    return this.http.request(method, url, options);
  }

  getHeaders(): HttpHeaders {
    return new HttpHeaders({
      Accept: 'application/json',
      // Authorization: 'Bearer ' + this.oidcSecurityService.getAccessToken(),
    });
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
