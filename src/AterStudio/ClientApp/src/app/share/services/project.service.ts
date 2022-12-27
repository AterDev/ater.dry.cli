import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { Project } from '../models/project/project.model';

/**
 * Project
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
   * project
   * @param id string
   */
  project(id: string): Observable<Project> {
    const url = `/api/Project/${id}`;
    return this.request<Project>('get', url);
  }

  /**
   * getWatcherStatus
   * @param id string
   */
  getWatcherStatus(id: string): Observable<boolean> {
    const url = `/api/Project/watcher/${id}`;
    return this.request<boolean>('get', url);
  }

  /**
   * startWatcher
   * @param id string
   */
  startWatcher(id: string): Observable<boolean> {
    const url = `/api/Project/watcher/${id}`;
    return this.request<boolean>('post', url);
  }

  /**
   * stopWatcher
   * @param id string
   */
  stopWatcher(id: string): Observable<boolean> {
    const url = `/api/Project/watcher/${id}`;
    return this.request<boolean>('delete', url);
  }

}
