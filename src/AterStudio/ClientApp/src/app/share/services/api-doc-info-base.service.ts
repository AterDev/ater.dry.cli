import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { ApiDocInfoAddDto } from '../models/api-doc-info/api-doc-info-add-dto.model';
import { ApiDocInfoUpdateDto } from '../models/api-doc-info/api-doc-info-update-dto.model';
import { CreateUIComponentDto } from '../models/api-doc-info/create-uicomponent-dto.model';
import { ApiDocInfoItemDto } from '../models/api-doc-info/api-doc-info-item-dto.model';
import { ApiDocInfo } from '../models/api-doc-info/api-doc-info.model';
import { ApiDocContent } from '../models/api-doc-info/api-doc-content.model';
import { NgComponentInfo } from '../models/api-doc-info/ng-component-info.model';
import { RequestLibType } from '../models/enum/request-lib-type.model';
import { LanguageType } from '../models/enum/language-type.model';

/**
 * api文档
 */
@Injectable({ providedIn: 'root' })
export class ApiDocInfoBaseService extends BaseService {
  /**
   * 获取项目文档
   */
  list(): Observable<ApiDocInfoItemDto[]> {
    const _url = `/api/ApiDocInfo`;
    return this.request<ApiDocInfoItemDto[]>('get', _url);
  }

  /**
   * 添加
   * @param data ApiDocInfoAddDto
   */
  add(data: ApiDocInfoAddDto): Observable<ApiDocInfo> {
    const _url = `/api/ApiDocInfo`;
    return this.request<ApiDocInfo>('post', _url, data);
  }

  /**
   * 获取某个文档信息
   * @param id 
   */
  getApiDocContent(id: string): Observable<ApiDocContent> {
    const _url = `/api/ApiDocInfo/${id}`;
    return this.request<ApiDocContent>('get', _url);
  }

  /**
   * 更新
   * @param id 
   * @param data ApiDocInfoUpdateDto
   */
  update(id: string, data: ApiDocInfoUpdateDto): Observable<ApiDocInfo> {
    const _url = `/api/ApiDocInfo/${id}`;
    return this.request<ApiDocInfo>('put', _url, data);
  }

  /**
   * 删除
   * @param id 
   */
  delete(id: string): Observable<ApiDocInfo> {
    const _url = `/api/ApiDocInfo/${id}`;
    return this.request<ApiDocInfo>('delete', _url);
  }

  /**
   * 导出markdown文档
   * @param id 
   */
  export(id: string): Observable<Blob> {
    const _url = `/api/ApiDocInfo/export/${id}`;
    return this.downloadFile('get', _url);
  }

  /**
   * 生成页面组件
   * @param data CreateUIComponentDto
   */
  createUIComponent(data: CreateUIComponentDto): Observable<NgComponentInfo> {
    const _url = `/api/ApiDocInfo/component`;
    return this.request<NgComponentInfo>('post', _url, data);
  }

  /**
   * 生成前端请求
   * @param id 
   * @param webPath 
   * @param type 
   * @param swaggerPath 
   */
  generateRequest(id: string, webPath: string | null, type: RequestLibType | null, swaggerPath: string | null): Observable<boolean> {
    const _url = `/api/ApiDocInfo/generateRequest/${id}?webPath=${webPath}&type=${type}&swaggerPath=${swaggerPath}`;
    return this.request<boolean>('get', _url);
  }

  /**
   * 生成客户端请求
   * @param id 
   * @param webPath 
   * @param type 
   * @param swaggerPath 
   */
  generateClientRequest(id: string, webPath: string | null, type: LanguageType | null, swaggerPath: string | null): Observable<boolean> {
    const _url = `/api/ApiDocInfo/generateClientRequest/${id}?webPath=${webPath}&type=${type}&swaggerPath=${swaggerPath}`;
    return this.request<boolean>('get', _url);
  }

}
