import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { AddEntityDto } from '../models/advance/add-entity-dto.model';

/**
 * 高级功能
 */
@Injectable({ providedIn: 'root' })
export class AdvanceBaseService extends BaseService {
  /**
   * 获取token
   * @param username 
   * @param password 
   */
  getToken(username: string | null, password: string | null): Observable<string> {
    const _url = `/api/Advance/token?username=${username ?? ''}&password=${password ?? ''}`;
    return this.request<string>('get', _url);
  }

  /**
   * 生成实体
   * @param name 
   * @param description 
   */
  getEntity(name: string | null, description: string | null): Observable<string[]> {
    const _url = `/api/Advance/entity?name=${name ?? ''}&description=${description ?? ''}`;
    return this.request<string[]>('get', _url);
  }

  /**
   * 创建实体
   * @param projectId 
   * @param data AddEntityDto
   */
  createEntity(projectId: string, data: AddEntityDto): Observable<boolean> {
    const _url = `/api/Advance/entity/${projectId}`;
    return this.request<boolean>('post', _url, data);
  }

}
