import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { GenerateDto } from '../models/entity/generate-dto.model';
import { BatchGenerateDto } from '../models/entity/batch-generate-dto.model';
import { EntityFile } from '../models/entity/entity-file.model';
import { RequestLibType } from '../models/enum/request-lib-type.model';

/**
 * Entity
 */
@Injectable({ providedIn: 'root' })
export class EntityService extends BaseService {
  /**
   * list
   * @param id string
   * @param name string
   */
  list(id: string, name?: string): Observable<EntityFile[]> {
    const url = `/api/Entity/${id}?name=${name}`;
    return this.request<EntityFile[]>('get', url);
  }

  /**
   * generate
   * @param data GenerateDto
   */
  generate(data: GenerateDto): Observable<boolean> {
    const url = `/api/Entity/generate`;
    return this.request<boolean>('post', url, data);
  }

  /**
   * batchGenerate
   * @param data BatchGenerateDto
   */
  batchGenerate(data: BatchGenerateDto): Observable<boolean> {
    const url = `/api/Entity/batch-generate`;
    return this.request<boolean>('post', url, data);
  }

  /**
   * generateRequest
   * @param id string
   * @param webPath string
   * @param type RequestLibType
   */
  generateRequest(id: string, webPath?: string, type?: RequestLibType): Observable<boolean> {
    const url = `/api/Entity/generateRequest/${id}?webPath=${webPath}&type=${type}`;
    return this.request<boolean>('get', url);
  }

  /**
   * generateSync
   * @param id string
   */
  generateSync(id: string): Observable<boolean> {
    const url = `/api/Entity/generateSync/${id}`;
    return this.request<boolean>('post', url);
  }

}
