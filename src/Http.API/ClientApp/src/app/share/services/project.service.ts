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
   * startWatcher
   * @param id number
   */
  startWatcher(id: number): Observable<boolean> {
    const url = `/api/Project/watcher/${id}`;
    return this.request<boolean>('post', url);
  }

  /**
   * stopWatcher
   * @param id number
   */
  stopWatcher(id: number): Observable<boolean> {
    const url = `/api/Project/watcher/${id}`;
    return this.request<boolean>('delete', url);
  }

}
