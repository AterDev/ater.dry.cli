import { Injectable } from '@angular/core';
import { EntityBaseService } from './entity-base.service';

/**
 * 实体
 */
@Injectable({providedIn: 'root' })
export class EntityService extends EntityBaseService {
  id: string | null = null;
  name: string | null = null;
}