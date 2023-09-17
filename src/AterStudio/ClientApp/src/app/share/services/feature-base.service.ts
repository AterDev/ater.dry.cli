import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { CreateSolutionDto } from '../models/feature/create-solution-dto.model';
import { SubProjectInfo } from '../models/feature/sub-project-info.model';

/**
 * 功能模块
 */
@Injectable({ providedIn: 'root' })
export class FeatureBaseService extends BaseService {
  /**
   * 创建新解决方案
   * @param data CreateSolutionDto
   */
  createNewSolution(data: CreateSolutionDto): Observable<boolean> {
    const url = `/api/Feature/newSolution`;
    return this.request<boolean>('post', url, data);
  }

  /**
   * 获取模块列表
   */
  getModulesInfo(): Observable<SubProjectInfo[]> {
    const url = `/api/Feature/modules`;
    return this.request<SubProjectInfo[]>('get', url);
  }

  /**
   * 创建模块
   * @param name 
   */
  createModule(name: string | null): Observable<boolean> {
    const url = `/api/Feature/createModule?name=${name??''}`;
    return this.request<boolean>('post', url);
  }

  /**
   * 重构Manager接口
   */
  removeIManager(): Observable<boolean> {
    const url = `/api/Feature/removeIManager`;
    return this.request<boolean>('put', url);
  }

}