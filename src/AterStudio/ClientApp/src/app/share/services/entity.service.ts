import { Injectable } from '@angular/core';
import { BaseService } from './base.service';
import { Observable } from 'rxjs';
import { UpdateDtoDto } from '../models/entity/update-dto-dto.model';
import { GenerateDto } from '../models/entity/generate-dto.model';
import { BatchGenerateDto } from '../models/entity/batch-generate-dto.model';
import { EntityFile } from '../models/entity/entity-file.model';
import { RequestLibType } from '../models/enum/request-lib-type.model';
import { LanguageType } from '../models/enum/language-type.model';

/**
 * 实体
 */
@Injectable({ providedIn: 'root' })
export class EntityService extends BaseService {
  /**
   * list
   * @param id string
   * @param name string
   */
  list(id: string, name: string | null): Observable<EntityFile[]> {
    const url = `/api/Entity/${id}?name=${name}`;
    return this.request<EntityFile[]>('get', url);
  }

  /**
   * s
            获取dtos
   * @param entityFilePath 
   */
  getDtos(entityFilePath: string | null): Observable<EntityFile[]> {
    const url = `/api/Entity/dtos?entityFilePath=${entityFilePath}`;
    return this.request<EntityFile[]>('get', url);
  }

  /**
   * 清理解决方案
   */
  cleanSolution(): Observable<string> {
    const url = `/api/Entity`;
    return this.request<string>('delete', url);
  }

  /**
   * 获取文件内容
entity/manager
   * @param entityName 
   * @param isManager 是否为manager
   * @param moduleName 
   */
  getFileContent(entityName: string | null, isManager: boolean | null, moduleName: string | null): Observable<EntityFile> {
    const url = `/api/Entity/fileContent?entityName=${entityName}&isManager=${isManager}&moduleName=${moduleName}`;
    return this.request<EntityFile>('get', url);
  }

  /**
   * 更新内容
   * @param data UpdateDtoDto
   */
  updateDtoContent(data: UpdateDtoDto): Observable<boolean> {
    const url = `/api/Entity/dto`;
    return this.request<boolean>('put', url, data);
  }

  /**
   * generate
   * @param data GenerateDto
   */
  generate(data: GenerateDto): Observable<boolean> {
    const url = `/api/Entity/generate`;
    return this.request<boolean>('post', url, data);
  }

  /**
   * 批量生成
   * @param data BatchGenerateDto
   */
  batchGenerate(data: BatchGenerateDto): Observable<boolean> {
    const url = `/api/Entity/batch-generate`;
    return this.request<boolean>('post', url, data);
  }

  /**
   * 生成前端请求
   * @param id 
   * @param webPath 
   * @param type 
   * @param swaggerPath 
   */
  generateRequest(id: string, webPath: string | null, type: RequestLibType | null, swaggerPath: string | null): Observable<boolean> {
    const url = `/api/Entity/generateRequest/${id}?webPath=${webPath}&type=${type}&swaggerPath=${swaggerPath}`;
    return this.request<boolean>('get', url);
  }

  /**
   * 生成客户端请求
   * @param id 
   * @param webPath 
   * @param type 
   * @param swaggerPath 
   */
  generateClientRequest(id: string, webPath: string | null, type: LanguageType | null, swaggerPath: string | null): Observable<boolean> {
    const url = `/api/Entity/generateClientRequest/${id}?webPath=${webPath}&type=${type}&swaggerPath=${swaggerPath}`;
    return this.request<boolean>('get', url);
  }

  /**
   * 同步ng页面
   * @param id 
   */
  generateSync(id: string): Observable<boolean> {
    const url = `/api/Entity/generateSync/${id}`;
    return this.request<boolean>('post', url);
  }

  /**
   * 生成NG组件模块
   * @param id 
   * @param entityName 
   * @param rootPath 
   */
  generateNgModule(id: string, entityName: string | null, rootPath: string | null): Observable<boolean> {
    const url = `/api/Entity/generateNgModule/${id}?entityName=${entityName}&rootPath=${rootPath}`;
    return this.request<boolean>('post', url);
  }

}
