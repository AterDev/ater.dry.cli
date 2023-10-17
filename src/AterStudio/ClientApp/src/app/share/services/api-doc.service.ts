import { Injectable } from '@angular/core';
import { ApiDocBaseService } from './api-doc-base.service';

/**
 * api文档
 */
@Injectable({providedIn: 'root' })
export class ApiDocService extends ApiDocBaseService {
  id: string | null = null;
  name: string | null = null;
}