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
    const url = `/api/Advance/config?key=${key??''}`;
    return this.request<ConfigData>('get', url);
  }

  /**
   * 设置配置
   * @param key 
   * @param value 
   */
  setConfig(key: string | null, value: string | null): Observable<any> {
    const url = `/api/Advance/config?key=${key??''}&value=${value??''}`;
    return this.request<any>('put', url);
  }

  /**
   * 生成实体
   * @param content 
   */
  generateEntity(content: string | null): Observable<any> {
    const url = `/api/Advance/generateEntity?content=${content??''}`;
    return this.request<any>('post', url);
  }

  /**
   * 问答
   * @param content 
   */
  getAnswer(content: string | null): Observable<any> {
    const url = `/api/Advance/answer?content=${content??''}`;
    return this.request<any>('get', url);
  }

  /**
   * 生成图片
   * @param content 
   */
  getImages(content: string | null): Observable<string[]> {
    const url = `/api/Advance/Images?content=${content??''}`;
    return this.request<string[]>('get', url);
  }

}
