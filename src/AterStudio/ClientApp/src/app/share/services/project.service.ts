import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { Project } from '../models/project/project.model';
import { SubProjectInfo } from '../models/project/sub-project-info.model';

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
   * add
   * @param name string
   * @param path string
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
   * getAllProjectInfos
   * @param id string
   */
  getAllProjectInfos(id: string): Observable<SubProjectInfo[]> {
    const url = `/api/Project/sub/${id}`;
    return this.request<SubProjectInfo[]>('get', url);
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
