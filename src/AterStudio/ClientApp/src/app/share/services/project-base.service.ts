import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { UpdateConfigOptionsDto } from '../models/project/update-config-options-dto.model';
import { TemplateFileUpsert } from '../models/project/template-file-upsert.model';
import { Project } from '../models/project/project.model';
import { SubProjectInfo } from '../models/feature/sub-project-info.model';
import { ConfigOptions } from '../models/project/config-options.model';
import { TemplateFile } from '../models/project/template-file.model';

/**
 * 项目
 */
@Injectable({ providedIn: 'root' })
export class ProjectBaseService extends BaseService {
  /**
   * 获取解决方案列表
   */
  list(): Observable<Project[]> {
    const _url = `/api/Project`;
    return this.request<Project[]>('get', _url);
  }

  /**
   * 添加项目
   * @param name 
   * @param path 
   */
  add(name: string | null, path: string | null): Observable<string> {
    const _url = `/api/Project?name=${name ?? ''}&path=${path ?? ''}`;
    return this.request<string>('post', _url);
  }

  /**
   * 获取工具版本
   */
  getVersion(): Observable<string> {
    const _url = `/api/Project/version`;
    return this.request<string>('get', _url);
  }

  /**
   * 详情
   * @param id 
   */
  project(id: string): Observable<Project> {
    const _url = `/api/Project/${id}`;
    return this.request<Project>('get', _url);
  }

  /**
   * 删除项目
   * @param id 
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/Project/${id}`;
    return this.request<boolean>('delete', _url);
  }

  /**
   * getAllProjectInfos
   * @param id string
   */
  getAllProjectInfos(id: string): Observable<SubProjectInfo[]> {
    const _url = `/api/Project/sub/${id}`;
    return this.request<SubProjectInfo[]>('get', _url);
  }

  /**
   * 添加微服务
   * @param name 
   */
  addService(name: string | null): Observable<boolean> {
    const _url = `/api/Project/service?name=${name ?? ''}`;
    return this.request<boolean>('post', _url);
  }

  /**
   * 获取项目配置文件内容
   */
  getConfigOptions(): Observable<ConfigOptions> {
    const _url = `/api/Project/setting`;
    return this.request<ConfigOptions>('get', _url);
  }

  /**
   * 更新配置
   * @param data UpdateConfigOptionsDto
   */
  updateConfig(data: UpdateConfigOptionsDto): Observable<boolean> {
    const _url = `/api/Project/setting`;
    return this.request<boolean>('put', _url, data);
  }

  /**
   * 更新解决方案
   */
  updateSolution(): Observable<string> {
    const _url = `/api/Project/solution`;
    return this.request<string>('put', _url);
  }

  /**
   * 打开解决方案，仅支持sln
   * @param path 
   */
  openSolution(path: string | null): Observable<string> {
    const _url = `/api/Project/open?path=${path ?? ''}`;
    return this.request<string>('post', _url);
  }

  /**
   * 获取模板名称
   * @param id string
   */
  getTemplateFiles(id: string): Observable<TemplateFile[]> {
    const _url = `/api/Project/tempaltes/${id}`;
    return this.request<TemplateFile[]>('get', _url);
  }

  /**
   * 获取模板内容
   * @param id 
   * @param name 
   */
  getTemplateFile(id: string, name: string | null): Observable<TemplateFile> {
    const _url = `/api/Project/template/${id}?name=${name ?? ''}`;
    return this.request<TemplateFile>('get', _url);
  }

  /**
   * 更新模板内容
   * @param id 
   * @param data TemplateFileUpsert
   */
  saveTemplateFile(id: string, data: TemplateFileUpsert): Observable<boolean> {
    const _url = `/api/Project/template/${id}`;
    return this.request<boolean>('post', _url, data);
  }

  /**
   * 获取实体表结构
   * @param id 
   */
  getDatabaseContent(id: string): Observable<string> {
    const _url = `/api/Project/database/${id}`;
    return this.request<string>('get', _url);
  }

}
