import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { AddEntityDto } from '../models/advance/add-entity-dto.model';
import { ConfigData } from '../models/advance/config-data.model';

/**
 * 高级功能
 */
@Injectable({ providedIn: 'root' })
export class AdvanceService extends BaseService {
  /**
   * 获取配置
   * @param key 
   */
  getConfig(key: string | null): Observable<ConfigData> {
    const url = `/api/Advance/config?key=${key}`;
    return this.request<ConfigData>('get', url);
  }

  /**
   * 设置配置
   * @param key 
   * @param value 
   */
  setConfig(key: string | null, value: string | null): Observable<any> {
    const url = `/api/Advance/config?key=${key}&value=${value}`;
    return this.request<any>('put', url);
  }

  /**
   * 生成实体
   * @param content 
   */
  generateEntity(content: string | null): Observable<any> {
    const url = `/api/Advance/generateEntity?content=${content}`;
    return this.request<any>('post', url);
  }

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

  /**
   * 创建实体
   * @param projectId 
   * @param data AddEntityDto
   */
  createEntity(projectId: string, data: AddEntityDto): Observable<boolean> {
    const url = `/api/Advance/entity/${projectId}`;
    return this.request<boolean>('post', url, data);
  }

}
