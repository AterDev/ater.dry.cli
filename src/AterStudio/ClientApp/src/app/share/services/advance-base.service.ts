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

  /**
   * 生成实体
   * @param content 
   */
  generateEntity(content: string | null): Observable<any> {
    const _url = `/api/Advance/generateEntity?content=${content ?? ''}`;
    return this.request<any>('post', _url);
  }

  /**
   * 问答
   * @param content 
   */
  getAnswer(content: string | null): Observable<any> {
    const _url = `/api/Advance/answer?content=${content ?? ''}`;
    return this.request<any>('get', _url);
  }

  /**
   * 生成图片
   * @param content 
   */
  getImages(content: string | null): Observable<string[]> {
    const _url = `/api/Advance/Images?content=${content ?? ''}`;
    return this.request<string[]>('get', _url);
  }

}
