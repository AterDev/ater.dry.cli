import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { ConfigData } from '../models/advance/config-data.model';

/**
 * 高级功能
 */
@Injectable({ providedIn: 'root' })
export class AdvanceBaseService extends BaseService {
  /**
   * 获取配置
   * @param key 
   */
  getConfig(key: string | null): Observable<ConfigData> {
    const _url = `/api/Advance/config?key=${key ?? ''}`;
    return this.request<ConfigData>('get', _url);
  }

  /**
   * 设置配置
   * @param key 
   * @param value 
   */
  setConfig(key: string | null, value: string | null): Observable<any> {
    const _url = `/api/Advance/config?key=${key ?? ''}&value=${value ?? ''}`;
    return this.request<any>('put', _url);
  }

}
