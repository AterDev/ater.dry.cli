import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { UpdateConfigOptionsDto } from '../models/project/update-config-options-dto.model';
import { Project } from '../models/project/project.model';
import { SubProjectInfo } from '../models/project/sub-project-info.model';
import { ConfigOptions } from '../models/project/config-options.model';

/**
 * 项目
 */
@Injectable({ providedIn: 'root' })
export class ProjectService extends BaseService {
  /**
   * list
   */
  list(): Observable<Project[]> {
    const url = `/api/Project`;
    return this.request<Project[]>('get', url);
  }

  /**
   * 添加项目
   * @param name 
   * @param path 
   */
  add(name?: string, path?: string): Observable<Project> {
    const url = `/api/Project?name=${name}&path=${path}`;
    return this.request<Project>('post', url);
  }

  /**
   * 详情
   * @param id 
   */
  project(id: string): Observable<Project> {
    const url = `/api/Project/${id}`;
    return this.request<Project>('get', url);
  }

  /**
   * 删除项目
   * @param id 
   */
  delete(id: string): Observable<boolean> {
    const url = `/api/Project/${id}`;
    return this.request<boolean>('delete', url);
  }

  /**
   * getAllProjectInfos
   * @param id string
   */
  getAllProjectInfos(id: string): Observable<SubProjectInfo[]> {
    const url = `/api/Project/sub/${id}`;
    return this.request<SubProjectInfo[]>('get', url);
  }

  /**
   * 获取项目配置文件内容
   * @param id 
   */
  getConfigOptions(id: string): Observable<ConfigOptions> {
    const url = `/api/Project/setting/${id}`;
    return this.request<ConfigOptions>('get', url);
  }

  /**
   * 更新配置
   * @param id 
   * @param data UpdateConfigOptionsDto
   */
  updateConfig(id: string, data: UpdateConfigOptionsDto): Observable<boolean> {
    const url = `/api/Project/setting/${id}`;
    return this.request<boolean>('put', url, data);
  }

  /**
   * 获取监听状态
   * @param id string
   */
  getWatcherStatus(id: string): Observable<boolean> {
    const url = `/api/Project/watcher/${id}`;
    return this.request<boolean>('get', url);
  }

  /**
   * 开启监测
   * @param id 
   */
  startWatcher(id: string): Observable<boolean> {
    const url = `/api/Project/watcher/${id}`;
    return this.request<boolean>('post', url);
  }

  /**
   * 停止监测
   * @param id 
   */
  stopWatcher(id: string): Observable<boolean> {
    const url = `/api/Project/watcher/${id}`;
    return this.request<boolean>('delete', url);
  }

}
