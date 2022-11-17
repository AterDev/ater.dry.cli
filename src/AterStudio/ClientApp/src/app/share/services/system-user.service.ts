import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { SystemUserFilterDto } from '../models/system-user/system-user-filter-dto.model';
import { SystemUserAddDto } from '../models/system-user/system-user-add-dto.model';
import { SystemUserUpdateDto } from '../models/system-user/system-user-update-dto.model';
import { SystemUserItemDtoPageList } from '../models/system-user/system-user-item-dto-page-list.model';
import { SystemUser } from '../models/system-user/system-user.model';
import { UploadResult } from '../models/system-user/upload-result.model';

/**
 * 系统用户
 */
@Injectable({ providedIn: 'root' })
export class SystemUserService extends BaseService {
  /**
   * 筛选
   * @param data SystemUserFilterDto
   */
  filter(data: SystemUserFilterDto): Observable<SystemUserItemDtoPageList> {
    const url = `/api/SystemUser/filter`;
    return this.request<SystemUserItemDtoPageList>('post', url, data);
  }

  /**
   * 新增
   * @param data SystemUserAddDto
   */
  add(data: SystemUserAddDto): Observable<SystemUser> {
    const url = `/api/SystemUser`;
    return this.request<SystemUser>('post', url, data);
  }

  /**
   * 更新
   * @param id 
   * @param data SystemUserUpdateDto
   */
  update(id: string, data: SystemUserUpdateDto): Observable<SystemUser> {
    const url = `/api/SystemUser/${id}`;
    return this.request<SystemUser>('put', url, data);
  }

  /**
   * 详情
   * @param id 
   */
  getDetail(id: string): Observable<SystemUser> {
    const url = `/api/SystemUser/${id}`;
    return this.request<SystemUser>('get', url);
  }

  /**
   * ⚠删除
   * @param id 
   */
  delete(id: string): Observable<SystemUser> {
    const url = `/api/SystemUser/${id}`;
    return this.request<SystemUser>('delete', url);
  }

  /**
   * 上传图片
   * @param data FormData
   */
  uploadImg(data: FormData): Observable<UploadResult> {
    const url = `/api/SystemUser/upload`;
    return this.request<UploadResult>('post', url, data);
  }

}
