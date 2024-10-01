import { Injectable } from '@angular/core';
import { BaseService } from '../base.service';
import { Observable } from 'rxjs';
import { UpdateDtoDto } from './models/update-dto-dto.model';
import { GenerateDto } from './models/generate-dto.model';
import { BatchGenerateDto } from './models/batch-generate-dto.model';
import { NgModuleDto } from './models/ng-module-dto.model';
import { EntityFile } from './models/entity-file.model';

/**
 * 实体
 */
@Injectable({ providedIn: 'root' })
export class EntityInfoBaseService extends BaseService {
  /**
   * list
   * @param id string
   */
  list(id: string): Observable<EntityFile[]> {
    const _url = `/api/admin/EntityInfo/${id}`;
    return this.request<EntityFile[]>('get', _url);
  }

  /**
   * s
            获取dtos
   * @param entityFilePath 
   */
  getDtos(entityFilePath: string | null): Observable<EntityFile[]> {
    const _url = `/api/admin/EntityInfo/dtos?entityFilePath=${entityFilePath ?? ''}`;
    return this.request<EntityFile[]>('get', _url);
  }

  /**
   * 创建DTO
   * @param entityFilePath 
   * @param name 
   * @param summary 
   */
  createDto(entityFilePath: string | null, name: string | null, summary: string | null): Observable<string> {
    const _url = `/api/admin/EntityInfo/dto?entityFilePath=${entityFilePath ?? ''}&name=${name ?? ''}&summary=${summary ?? ''}`;
    return this.request<string>('post', _url);
  }

  /**
   * 更新内容
   * @param data UpdateDtoDto
   */
  updateDtoContent(data: UpdateDtoDto): Observable<boolean> {
    const _url = `/api/admin/EntityInfo/dto`;
    return this.request<boolean>('put', _url, data);
  }

  /**
   * 清理解决方案
   */
  cleanSolution(): Observable<string> {
    const _url = `/api/admin/EntityInfo`;
    return this.request<string>('delete', _url);
  }

  /**
   * 获取文件内容 entity/manager
   * @param entityName 
   * @param isManager 是否为manager
   * @param moduleName 
   */
  getFileContent(entityName: string | null, isManager: boolean | null, moduleName: string | null): Observable<EntityFile> {
    const _url = `/api/admin/EntityInfo/fileContent?entityName=${entityName ?? ''}&isManager=${isManager ?? ''}&moduleName=${moduleName ?? ''}`;
    return this.request<EntityFile>('get', _url);
  }

  /**
   * generate
   * @param data GenerateDto
   */
  generate(data: GenerateDto): Observable<boolean> {
    const _url = `/api/admin/EntityInfo/generate`;
    return this.request<boolean>('post', _url, data);
  }

  /**
   * 批量生成
   * @param data BatchGenerateDto
   */
  batchGenerate(data: BatchGenerateDto): Observable<boolean> {
    const _url = `/api/admin/EntityInfo/batch-generate`;
    return this.request<boolean>('post', _url, data);
  }

  /**
   * 同步ng页面
   * @param id 
   */
  generateSync(id: string): Observable<boolean> {
    const _url = `/api/admin/EntityInfo/generateSync/${id}`;
    return this.request<boolean>('post', _url);
  }

  /**
   * 生成NG组件模块
   * @param data NgModuleDto
   */
  generateNgModule(data: NgModuleDto): Observable<boolean> {
    const _url = `/api/admin/EntityInfo/generateNgModule`;
    return this.request<boolean>('post', _url, data);
  }

}
