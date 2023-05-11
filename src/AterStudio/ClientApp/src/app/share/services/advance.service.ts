import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { AddEntityDto } from '../models/advance/add-entity-dto.model';

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
