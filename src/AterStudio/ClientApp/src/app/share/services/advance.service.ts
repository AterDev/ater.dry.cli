import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';

/**
 * Advance
 */
@Injectable({ providedIn: 'root' })
export class AdvanceService extends BaseService {
  /**
   * getToken
   * @param username string
   * @param password string
   */
  getToken(username?: string, password?: string): Observable<string> {
    const url = `/api/Advance/token?username=${username}&password=${password}`;
    return this.request<string>('get', url);
  }

  /**
   * getEntity
   * @param name string
   * @param description string
   */
  getEntity(name?: string, description?: string): Observable<string[]> {
    const url = `/api/Advance/entity?name=${name}&description=${description}`;
    return this.request<string[]>('get', url);
  }

}
