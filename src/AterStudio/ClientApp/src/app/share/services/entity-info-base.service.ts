import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { UpdateDtoDto } from '../models/entity-info/update-dto-dto.model';
import { GenerateDto } from '../models/entity-info/generate-dto.model';
import { BatchGenerateDto } from '../models/entity-info/batch-generate-dto.model';
import { EntityFile } from '../models/entity-info/entity-file.model';

/**
 * 实体
 */
@Injectable({ providedIn: 'root' })
export class EntityInfoBaseService extends BaseService {
  /**
   * list
   * @param id string
   * @param serviceName string
   */
  list(id: string, serviceName: string | null): Observable<EntityFile[]> {
    const _url = `/api/EntityInfo/${id}?serviceName=${serviceName ?? ''}`;
    return this.request<EntityFile[]>('get', _url);
  }

  /**
   * s
            获取dtos
   * @param entityFilePath 
   */
  getDtos(entityFilePath: string | null): Observable<EntityFile[]> {
    const _url = `/api/EntityInfo/dtos?entityFilePath=${entityFilePath ?? ''}`;
    return this.request<EntityFile[]>('get', _url);
  }

  /**
   * 创建DTO
   * @param entityFilePath 
   * @param name 
   * @param summary 
   */
  createDto(entityFilePath: string | null, name: string | null, summary: string | null): Observable<string> {
    const _url = `/api/EntityInfo/dto?entityFilePath=${entityFilePath ?? ''}&name=${name ?? ''}&summary=${summary ?? ''}`;
    return this.request<string>('post', _url);
  }

  /**
   * 更新内容
   * @param data UpdateDtoDto
   */
  updateDtoContent(data: UpdateDtoDto): Observable<boolean> {
    const _url = `/api/EntityInfo/dto`;
    return this.request<boolean>('put', _url, data);
  }

  /**
   * 清理解决方案
   */
  cleanSolution(): Observable<string> {
    const _url = `/api/EntityInfo`;
    return this.request<string>('delete', _url);
  }

  /**
   * 获取文件内容 entity/manager
   * @param entityName 
   * @param isManager 是否为manager
   * @param moduleName 
   */
  getFileContent(entityName: string | null, isManager: boolean | null, moduleName: string | null): Observable<EntityFile> {
    const _url = `/api/EntityInfo/fileContent?entityName=${entityName ?? ''}&isManager=${isManager ?? ''}&moduleName=${moduleName ?? ''}`;
    return this.request<EntityFile>('get', _url);
  }

  /**
   * generate
   * @param data GenerateDto
   */
  generate(data: GenerateDto): Observable<boolean> {
    const _url = `/api/EntityInfo/generate`;
    return this.request<boolean>('post', _url, data);
  }

  /**
   * 批量生成
   * @param data BatchGenerateDto
   */
  batchGenerate(data: BatchGenerateDto): Observable<boolean> {
    const _url = `/api/EntityInfo/batch-generate`;
    return this.request<boolean>('post', _url, data);
  }

  /**
   * 同步ng页面
   * @param id 
   */
  generateSync(id: string): Observable<boolean> {
    const _url = `/api/EntityInfo/generateSync/${id}`;
    return this.request<boolean>('post', _url);
  }

  /**
   * 生成NG组件模块
   * @param entityName 
   * @param rootPath 
   */
  generateNgModule(entityName: string | null, rootPath: string | null): Observable<boolean> {
    const _url = `/api/EntityInfo/generateNgModule?entityName=${entityName ?? ''}&rootPath=${rootPath ?? ''}`;
    return this.request<boolean>('post', _url);
  }

}
