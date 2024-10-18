import { Injectable } from '@angular/core';
import { BaseService } from '../base.service';
import { Observable } from 'rxjs';
import { GenActionFilterDto } from './models/gen-action-filter-dto.model';
import { GenActionAddDto } from './models/gen-action-add-dto.model';
import { GenActionUpdateDto } from './models/gen-action-update-dto.model';
import { GenActionItemDtoPageList } from './models/gen-action-item-dto-page-list.model';
import { GenActionDetailDto } from './models/gen-action-detail-dto.model';

/**
 * The project's generate action
 */
@Injectable({ providedIn: 'root' })
export class GenActionBaseService extends BaseService {
  /**
   * 分页数据 🛑
   * @param data GenActionFilterDto
   */
  filter(data: GenActionFilterDto): Observable<GenActionItemDtoPageList> {
    const _url = `/api/admin/GenAction/filter`;
    return this.request<GenActionItemDtoPageList>('post', _url, data);
  }

  /**
   * 新增 🛑
   * @param data GenActionAddDto
   */
  add(data: GenActionAddDto): Observable<string> {
    const _url = `/api/admin/GenAction`;
    return this.request<string>('post', _url, data);
  }

  /**
   * 更新数据 🛑
   * @param id 
   * @param data GenActionUpdateDto
   */
  update(id: string, data: GenActionUpdateDto): Observable<boolean> {
    const _url = `/api/admin/GenAction/${id}`;
    return this.request<boolean>('patch', _url, data);
  }

  /**
   * 获取详情 🛑
   * @param id 
   */
  getDetail(id: string): Observable<GenActionDetailDto> {
    const _url = `/api/admin/GenAction/${id}`;
    return this.request<GenActionDetailDto>('get', _url);
  }

  /**
   * 删除 🛑
   * @param id 
   */
  delete(id: string): Observable<boolean> {
    const _url = `/api/admin/GenAction/${id}`;
    return this.request<boolean>('delete', _url);
  }

}
