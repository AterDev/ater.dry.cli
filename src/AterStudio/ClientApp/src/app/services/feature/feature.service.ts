import { Injectable } from '@angular/core';
import { FeatureBaseService } from './feature-base.service';

/**
 * 功能模块
 */
@Injectable({providedIn: 'root' })
export class FeatureService extends FeatureBaseService {
  id: string | null = null;
  name: string | null = null;
}