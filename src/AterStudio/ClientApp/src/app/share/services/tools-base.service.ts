import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { ConvertDto } from '../models/tools/convert-dto.model';

/**
 * Tools
 */
@Injectable({ providedIn: 'root' })
export class ToolsBaseService extends BaseService {
  /**
   * 转换成类
   * @param data ConvertDto
   */
  convertToClass(data: ConvertDto): Observable<string[]> {
    const _url = `/api/Tools/classModel`;
    return this.request<string[]>('post', _url, data);
  }

}
