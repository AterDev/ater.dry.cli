import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
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

}
