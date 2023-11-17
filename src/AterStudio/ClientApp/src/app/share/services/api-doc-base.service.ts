import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { ApiDocInfo } from '../models/api-doc/api-doc-info.model';
import { CreateUIComponentDto } from '../models/api-doc/create-uicomponent-dto.model';
import { ApiDocContent } from '../models/api-doc/api-doc-content.model';
import { NgComponentInfo } from '../models/api-doc/ng-component-info.model';

/**
 * api文档
 */
@Injectable({ providedIn: 'root' })
export class ApiDocBaseService extends BaseService {
  /**
   * 获取项目文档
   * @param id 项目id
   */
  list(id: string): Observable<ApiDocInfo[]> {
    const _url = `/api/ApiDoc/all/${id}`;
    return this.request<ApiDocInfo[]>('get', _url);
  }

  /**
   * 获取某个文档信息
   * @param id 
   */
  getApiDocContent(id: string): Observable<ApiDocContent> {
    const _url = `/api/ApiDoc/${id}`;
    return this.request<ApiDocContent>('get', _url);
  }

  /**
   * 更新
   * @param id 
   * @param data ApiDocInfo
   */
  update(id: string, data: ApiDocInfo): Observable<ApiDocInfo> {
    const _url = `/api/ApiDoc/${id}`;
    return this.request<ApiDocInfo>('put', _url, data);
  }

  /**
   * 删除
   * @param id 
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/ApiDoc/${id}`;
    return this.request<boolean>('delete', _url);
  }

  /**
   * 导出markdown文档
   * @param id 
   */
  export(id: string): Observable<FormData> {
    const _url = `/api/ApiDoc/export/${id}`;
    return this.request<FormData>('get', _url);
  }

  /**
   * 添加
   * @param data ApiDocInfo
   */
  add(data: ApiDocInfo): Observable<ApiDocInfo> {
    const _url = `/api/ApiDoc`;
    return this.request<ApiDocInfo>('post', _url, data);
  }

  /**
   * 生成页面组件
   * @param data CreateUIComponentDto
   */
  createUIComponent(data: CreateUIComponentDto): Observable<NgComponentInfo> {
    const _url = `/api/ApiDoc/component`;
    return this.request<NgComponentInfo>('post', _url, data);
  }

}
