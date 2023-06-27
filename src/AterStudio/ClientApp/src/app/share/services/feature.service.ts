import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';

/**
 * 功能模块
 */
@Injectable({ providedIn: 'root' })
export class FeatureService extends BaseService {
  /**
   * 创建新解决方案
   * @param name 
   * @param path 
   */
  createNewSolution(name: string | null, path: string | null): Observable<boolean> {
    const url = `/api/Feature/newSolution?name=${name}&path=${path}`;
    return this.request<boolean>('post', url);
  }

}
