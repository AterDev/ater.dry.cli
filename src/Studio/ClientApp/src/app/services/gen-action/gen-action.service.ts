import { Injectable } from '@angular/core';
import { GenActionBaseService } from './gen-action-base.service';

/**
 * The project's generate action
 */
@Injectable({providedIn: 'root' })
export class GenActionService extends GenActionBaseService {
  id: string | null = null;
  name: string | null = null;
}