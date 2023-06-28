import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { CreateSolutionDto } from '../models/feature/create-solution-dto.model';

/**
 * 功能模块
 */
@Injectable({ providedIn: 'root' })
export class FeatureService extends BaseService {
  /**
   * 创建新解决方案
   * @param data CreateSolutionDto
   */
  createNewSolution(data: CreateSolutionDto): Observable<boolean> {
    const url = `/api/Feature/newSolution`;
    return this.request<boolean>('post', url, data);
  }

}
