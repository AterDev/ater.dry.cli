import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';

        import { ApiDocInfo } from '../models/api-doc/api-doc-info.model';
import { ApiDocContent } from '../models/api-doc/api-doc-content.model';

/**
 * api文档
 */
@Injectable({ providedIn: 'root' })
export class ApiDocService extends BaseService {
  /**
   * 获取项目文档
   * @param id 项目id
   */
  list(id: string): Observable<ApiDocInfo[]> {
    const url = `/api/ApiDoc/all/${id}`;
    return this.request<ApiDocInfo[]>('get', url);
  }

  /**
   * 获取某个文档信息
   * @param id 
   */
  getApiDocContent(id: string): Observable<ApiDocContent> {
    const url = `/api/ApiDoc/${id}`;
    return this.request<ApiDocContent>('get', url);
  }

  /**
   * 删除
   * @param id 
   */
  delete(id: string): Observable<boolean> {
    const url = `/api/ApiDoc/${id}`;
    return this.request<boolean>('delete', url);
  }

  /**
   * 添加
   * @param data ApiDocInfo
   */
  add(data: ApiDocInfo): Observable<ApiDocInfo> {
    const url = `/api/ApiDoc`;
    return this.request<ApiDocInfo>('post', url, data);
  }

}
