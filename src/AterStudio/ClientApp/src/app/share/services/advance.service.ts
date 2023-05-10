import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';

/**
 * Advance
 */
@Injectable({ providedIn: 'root' })
export class AdvanceService extends BaseService {
  /**
   * 获取token
   * @param username 
   * @param password 
   */
  getToken(username: string | null, password: string | null): Observable<string> {
    const url = `/api/Advance/token?username=${username}&password=${password}`;
    return this.request<string>('get', url);
  }

  /**
   * 生成实体
   * @param name 
   * @param description 
   */
  getEntity(name: string | null, description: string | null): Observable<string[]> {
    const url = `/api/Advance/entity?name=${name}&description=${description}`;
    return this.request<string[]>('get', url);
  }

}
