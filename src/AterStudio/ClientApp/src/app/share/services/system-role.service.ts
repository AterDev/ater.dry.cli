import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { SystemRoleFilterDto } from '../models/system-role/system-role-filter-dto.model';
import { SystemRoleAddDto } from '../models/system-role/system-role-add-dto.model';
import { SystemRoleUpdateDto } from '../models/system-role/system-role-update-dto.model';
import { SystemRoleItemDtoPageList } from '../models/system-role/system-role-item-dto-page-list.model';
import { SystemRole } from '../models/system-role/system-role.model';

/**
 * 角色表
 */
@Injectable({ providedIn: 'root' })
export class SystemRoleService extends BaseService {
  /**
   * 筛选
   * @param data SystemRoleFilterDto
   */
  filter(data: SystemRoleFilterDto): Observable<SystemRoleItemDtoPageList> {
    const url = `/api/SystemRole/filter`;
    return this.request<SystemRoleItemDtoPageList>('post', url, data);
  }

  /**
   * 新增
   * @param data SystemRoleAddDto
   */
  add(data: SystemRoleAddDto): Observable<SystemRole> {
    const url = `/api/SystemRole`;
    return this.request<SystemRole>('post', url, data);
  }

  /**
   * 更新
   * @param id 
   * @param data SystemRoleUpdateDto
   */
  update(id: string, data: SystemRoleUpdateDto): Observable<SystemRole> {
    const url = `/api/SystemRole/${id}`;
    return this.request<SystemRole>('put', url, data);
  }

  /**
   * 详情
   * @param id 
   */
  getDetail(id: string): Observable<SystemRole> {
    const url = `/api/SystemRole/${id}`;
    return this.request<SystemRole>('get', url);
  }

  /**
   * ⚠删除
   * @param id 
   */
  delete(id: string): Observable<SystemRole> {
    const url = `/api/SystemRole/${id}`;
    return this.request<SystemRole>('delete', url);
  }

}
