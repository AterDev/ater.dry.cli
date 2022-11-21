import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { GenerateDto } from '../models/entity/generate-dto.model';
import { EntityFile } from '../models/entity/entity-file.model';

/**
 * Entity
 */
@Injectable({ providedIn: 'root' })
export class EntityService extends BaseService {
  /**
   * list
   * @param id number
   */
  list(id: number): Observable<EntityFile[]> {
    const url = `/api/Entity/${id}`;
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

}
